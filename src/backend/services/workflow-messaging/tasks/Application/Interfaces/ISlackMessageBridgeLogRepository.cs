using System;
using System.Threading;
using System.Threading.Tasks;
using SlackMessageBridgeLogEntity = Harvestry.Tasks.Domain.Entities.SlackMessageBridgeLog;

namespace Harvestry.Tasks.Application.Interfaces;

public interface ISlackMessageBridgeLogRepository
{
    Task<SlackMessageBridgeLogEntity?> GetByRequestIdAsync(string requestId, Guid slackWorkspaceId, CancellationToken cancellationToken);
    Task AddAsync(SlackMessageBridgeLogEntity logEntry, CancellationToken cancellationToken);
    Task UpdateAsync(SlackMessageBridgeLogEntity logEntry, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
