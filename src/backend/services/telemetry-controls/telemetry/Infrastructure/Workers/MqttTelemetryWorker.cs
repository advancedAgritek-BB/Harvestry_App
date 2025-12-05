using System;
using System.Threading;
using System.Buffers;
using System.Threading.Tasks;
using Harvestry.Telemetry.Application.DeviceAdapters;
using Harvestry.Telemetry.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Formatter;
using MQTTnet.Protocol;

namespace Harvestry.Telemetry.Infrastructure.Workers;

/// <summary>
/// Background worker that listens for telemetry over MQTT and forwards payloads to the ingest pipeline.
/// </summary>
public sealed class MqttTelemetryWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<TelemetryMqttOptions> _optionsMonitor;
    private readonly ILogger<MqttTelemetryWorker> _logger;
    private IMqttClient? _client;

    public MqttTelemetryWorker(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<TelemetryMqttOptions> optionsMonitor,
        ILogger<MqttTelemetryWorker> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = _optionsMonitor.CurrentValue;
        if (!options.Enabled)
        {
            _logger.LogInformation("MQTT telemetry listener disabled via configuration.");
            return;
        }

        var factory = new MqttClientFactory();
        _client = factory.CreateMqttClient();
        _client.ApplicationMessageReceivedAsync += args => HandleMessageAsync(args, stoppingToken);

        _client.DisconnectedAsync += async args =>
        {
            if (args.ClientWasConnected)
            {
                _logger.LogWarning("MQTT telemetry listener disconnected: {Reason}.", args.ReasonString);
            }

            await Task.CompletedTask;
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            options = _optionsMonitor.CurrentValue;

            try
            {
                if (_client.IsConnected)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken).ConfigureAwait(false);
                    continue;
                }

                var clientId = options.ResolveClientId();
                var optionsBuilder = new MqttClientOptionsBuilder()
                    .WithClientId(clientId)
                    .WithCleanSession(true)
                    .WithProtocolVersion(MqttProtocolVersion.V500)
                    .WithTcpServer(options.Host, options.Port);

                if (!string.IsNullOrWhiteSpace(options.Username))
                {
                    optionsBuilder = optionsBuilder.WithCredentials(options.Username, options.Password ?? string.Empty);
                }

                if (options.UseTls)
                {
                    optionsBuilder = optionsBuilder.WithTlsOptions(tls => tls.UseTls(true));
                }

                var clientOptions = optionsBuilder.Build();

                _logger.LogInformation("Connecting to MQTT broker {Host}:{Port} as {ClientId}...", options.Host, options.Port, clientId);
                await _client.ConnectAsync(clientOptions, stoppingToken).ConfigureAwait(false);

                var topicFilter = string.IsNullOrWhiteSpace(options.TopicFilter)
                    ? "site/+/equipment/+/telemetry/#"
                    : options.TopicFilter;

                var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                    .WithTopicFilter(f => f.WithTopic(topicFilter).WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce))
                    .Build();

                await _client.SubscribeAsync(subscribeOptions, stoppingToken).ConfigureAwait(false);
                _logger.LogInformation("Subscribed to MQTT topic filter '{TopicFilter}'.", topicFilter);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                var delaySeconds = Math.Clamp(options.ReconnectIntervalSeconds, 1, 60);
                _logger.LogError(ex, "MQTT telemetry listener encountered an error. Retrying in {DelaySeconds} seconds...", delaySeconds);

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        if (_client.IsConnected)
        {
            try
            {
                var disconnectOptions = new MqttClientDisconnectOptionsBuilder()
                    .WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection)
                    .Build();

                await _client.DisconnectAsync(disconnectOptions, CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error while disconnecting MQTT client.");
            }
        }
    }

    private async Task HandleMessageAsync(MqttApplicationMessageReceivedEventArgs args, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var adapter = scope.ServiceProvider.GetRequiredService<IMqttIngestAdapter>();
            var topic = args.ApplicationMessage.Topic;
            var payload = args.ApplicationMessage.Payload.ToArray();

            await adapter.HandleAsync(topic, payload, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Cancellation requested, ignore.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process MQTT message for topic {Topic}.", args.ApplicationMessage.Topic);
        }
    }
}
