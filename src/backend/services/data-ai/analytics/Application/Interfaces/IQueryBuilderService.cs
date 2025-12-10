using Harvestry.Analytics.Domain.ValueObjects;

namespace Harvestry.Analytics.Application.Interfaces;

public interface IQueryBuilderService
{
    Task<IEnumerable<dynamic>> ExecuteQueryAsync(ReportConfig config, Guid userId, CancellationToken cancellationToken = default);
}




