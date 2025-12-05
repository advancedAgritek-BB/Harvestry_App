using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Harvestry.Tasks.Application.Interfaces;
using Harvestry.Tasks.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SlackNotificationEntity = Harvestry.Tasks.Domain.Entities.SlackNotification;
using SlackMessageBridgeLog = Harvestry.Tasks.Domain.Entities.SlackMessageBridgeLog;

namespace Harvestry.Tasks.Infrastructure.Workers;

public sealed class SlackNotificationWorker : BackgroundService
{
    private const int DefaultBatchSize = 25;
    private static readonly TimeSpan DefaultPollInterval = TimeSpan.FromSeconds(10);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SlackNotificationWorker> _logger;

    public SlackNotificationWorker(IServiceScopeFactory scopeFactory, ILogger<SlackNotificationWorker> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Slack notification worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // graceful shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Slack notification worker encountered an error");
            }

            await Task.Delay(DefaultPollInterval, stoppingToken).ConfigureAwait(false);
        }

        _logger.LogInformation("Slack notification worker stopped");
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var queueRepository = scope.ServiceProvider.GetRequiredService<ISlackNotificationQueueRepository>();
        var workspaceRepository = scope.ServiceProvider.GetRequiredService<ISlackWorkspaceRepository>();
        var bridgeLogRepository = scope.ServiceProvider.GetRequiredService<ISlackMessageBridgeLogRepository>();
        var slackApiClient = scope.ServiceProvider.GetRequiredService<ISlackApiClient>();

        var notifications = await queueRepository
            .GetPendingNotificationsAsync(DefaultBatchSize, DateTimeOffset.UtcNow, cancellationToken)
            .ConfigureAwait(false);

        if (notifications.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Processing {Count} Slack notifications", notifications.Count);

        foreach (var notification in notifications)
        {
            await HandleNotificationAsync(notification, queueRepository, workspaceRepository, bridgeLogRepository, slackApiClient, cancellationToken)
                .ConfigureAwait(false);
        }

        using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            await queueRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await bridgeLogRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            transactionScope.Complete();
        }
    }

    private async Task HandleNotificationAsync(
        SlackNotificationEntity notification,
        ISlackNotificationQueueRepository queueRepository,
        ISlackWorkspaceRepository workspaceRepository,
        ISlackMessageBridgeLogRepository bridgeLogRepository,
        ISlackApiClient slackApiClient,
        CancellationToken cancellationToken)
    {
        var existingLog = await bridgeLogRepository
            .GetByRequestIdAsync(notification.RequestId, notification.SlackWorkspaceId, cancellationToken)
            .ConfigureAwait(false)
            ;
        var isNewLog = existingLog is null;
        var bridgeLog = existingLog ?? CreateBridgeLog(notification);

        try
        {
            notification.MarkProcessing();
            notification.IncrementAttempt();

            var workspace = await workspaceRepository
                .GetByIdAsync(notification.SiteId, notification.SlackWorkspaceId, cancellationToken)
                .ConfigureAwait(false);

            if (workspace is null || string.IsNullOrWhiteSpace(workspace.EncryptedBotToken))
            {
                throw new InvalidOperationException(
                    $"Slack workspace {notification.SlackWorkspaceId} missing or inactive for site {notification.SiteId}.");
            }

            var response = await slackApiClient
                .SendMessageAsync(workspace.EncryptedBotToken!, notification.ChannelId, notification.PayloadJson, cancellationToken)
                .ConfigureAwait(false);

            notification.MarkSent();

            bridgeLog.RecordAttempt(
                response.ReceivedAt,
                SlackMessageBridgeStatus.Sent,
                response.ChannelId,
                string.IsNullOrWhiteSpace(response.Timestamp) ? null : response.Timestamp,
                response.ThreadTimestamp,
                null);
            bridgeLog.MarkSent(response.ReceivedAt, response.ChannelId, response.Timestamp, response.ThreadTimestamp);

            if (isNewLog)
            {
                await bridgeLogRepository.AddAsync(bridgeLog, cancellationToken).ConfigureAwait(false);
                isNewLog = false;
            }
            else
            {
                await bridgeLogRepository.UpdateAsync(bridgeLog, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            notification.MarkFailed(ex.Message);

            if (!notification.CanRetry())
            {
                notification.MarkDeadLetter(ex.Message);
            }
            else
            {
                var backoffSeconds = Math.Clamp(Math.Pow(2, notification.AttemptCount), 5, 300);
                notification.ScheduleRetry(TimeSpan.FromSeconds(backoffSeconds));
            }

            _logger.LogError(ex, "Failed to process Slack notification {NotificationId}", notification.Id);

            var attemptTime = DateTimeOffset.UtcNow;
            bridgeLog.RecordAttempt(
                attemptTime,
                SlackMessageBridgeStatus.Failed,
                notification.ChannelId,
                null,
                null,
                ex.Message);
            bridgeLog.MarkFailed(ex.Message);

            if (isNewLog)
            {
                await bridgeLogRepository.AddAsync(bridgeLog, cancellationToken).ConfigureAwait(false);
                isNewLog = false;
            }
            else
            {
                await bridgeLogRepository.UpdateAsync(bridgeLog, cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            await queueRepository.UpdateAsync(notification, cancellationToken).ConfigureAwait(false);
        }
    }

    private static SlackMessageBridgeLog CreateBridgeLog(SlackNotificationEntity notification)
    {
        var log = SlackMessageBridgeLog.Create(
            notification.SiteId,
            notification.SlackWorkspaceId,
            internalMessageId: notification.Id,
            internalMessageType: notification.NotificationType.ToString(),
            slackChannelId: notification.ChannelId,
            requestId: notification.RequestId);

        return log;
    }
}
