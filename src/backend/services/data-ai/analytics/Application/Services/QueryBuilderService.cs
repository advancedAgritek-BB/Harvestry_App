using Dapper;
using Harvestry.Analytics.Application.Interfaces;
using Harvestry.Analytics.Domain.ValueObjects;
using Harvestry.Analytics.Infrastructure.Persistence;
using System.Text;

namespace Harvestry.Analytics.Application.Services;

public class QueryBuilderService : IQueryBuilderService
{
    private readonly AnalyticsDbContext _dbContext;

    public QueryBuilderService(AnalyticsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private static readonly Dictionary<string, string> TableMap = new()
    {
        { "harvests", "analytics.vw_harvests_flat" },
        { "tasks", "analytics.vw_tasks_flat" }
    };
    
    public async Task<IEnumerable<dynamic>> ExecuteQueryAsync(ReportConfig config, Guid userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(config.Source) || !TableMap.TryGetValue(config.Source.ToLower(), out var tableName))
        {
            throw new ArgumentException($"Invalid source: {config.Source}");
        }

        var sqlBuilder = new StringBuilder();
        sqlBuilder.Append("SELECT ");

        if (config.Columns == null || !config.Columns.Any())
        {
            sqlBuilder.Append("*");
        }
        else
        {
            var columns = config.Columns.Select(c => 
            {
                ValidateIdentifier(c.Field);
                // Handle aggregation
                if (!string.IsNullOrEmpty(c.Aggregation))
                {
                    // Basic aggregations whitelist
                    var agg = c.Aggregation.ToUpper();
                    if (!new[] { "COUNT", "SUM", "AVG", "MIN", "MAX" }.Contains(agg))
                        throw new ArgumentException($"Invalid aggregation: {agg}");
                        
                    return $"{agg}({c.Field}) AS \"{c.Alias ?? c.Field}\"";
                }
                return $"{c.Field} AS \"{c.Alias ?? c.Field}\"";
            });
            sqlBuilder.Append(string.Join(", ", columns));
        }

        sqlBuilder.Append($" FROM {tableName} ");

        var parameters = new DynamicParameters();
        
        // Filters
        if (config.Filters != null && config.Filters.Any())
        {
            sqlBuilder.Append(" WHERE ");
            var filterClauses = new List<string>();
            int i = 0;
            foreach (var filter in config.Filters)
            {
                ValidateIdentifier(filter.Field);
                var paramName = $"@p{i}";
                var op = GetSqlOperator(filter.Operator);
                
                filterClauses.Add($"{filter.Field} {op} {paramName}");
                parameters.Add(paramName, filter.Value);
                i++;
            }
            sqlBuilder.Append(string.Join(" AND ", filterClauses));
        }

        // Group By
        if (config.Columns != null && config.Columns.Any(c => !string.IsNullOrEmpty(c.Aggregation)))
        {
            var groupByFields = config.Columns
                .Where(c => string.IsNullOrEmpty(c.Aggregation))
                .Select(c => c.Field);
                
            if (groupByFields.Any())
            {
                sqlBuilder.Append(" GROUP BY " + string.Join(", ", groupByFields));
            }
        }

        // Sorts
        if (config.Sorts != null && config.Sorts.Any())
        {
            sqlBuilder.Append(" ORDER BY ");
            var sortClauses = config.Sorts.Select(s => 
            {
                ValidateIdentifier(s.Field);
                var dir = s.Direction?.ToUpper() == "DESC" ? "DESC" : "ASC";
                return $"{s.Field} {dir}";
            });
            sqlBuilder.Append(string.Join(", ", sortClauses));
        }
        
        // RLS Context is applied by DbContext when opening connection
        // We must ensure context is set before querying
        // Since GetOpenConnectionAsync doesn't force RLS set if connection already open, we rely on Caller/Middleware to set RLS on the context?
        // Wait, GeneticsDbContext has SetRlsContextAsync. 
        // We should call that? Usually Middleware calls SetRlsContextAsync on the scoped DbContext.
        // I will assume the DI container provides a scoped AnalyticsDbContext that has been initialized by middleware.
        
        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        
        return await connection.QueryAsync(sqlBuilder.ToString(), parameters);
    }

    private void ValidateIdentifier(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier) || !identifier.All(c => char.IsLetterOrDigit(c) || c == '_'))
        {
            throw new ArgumentException($"Invalid identifier: {identifier}");
        }
    }

    private string GetSqlOperator(string op)
    {
        return op?.ToLower() switch
        {
            "eq" => "=",
            "neq" => "!=",
            "gt" => ">",
            "gte" => ">=",
            "lt" => "<",
            "lte" => "<=",
            "like" => "LIKE",
            "ilike" => "ILIKE",
            _ => "="
        };
    }
}





