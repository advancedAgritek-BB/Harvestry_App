using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;
using NpgsqlTypes;

namespace Harvestry.Sales.Infrastructure.Persistence.Rls;

/// <summary>
/// Applies PostgreSQL session variables for RLS on each opened connection.
/// This keeps RLS enforcement working with EF Core contexts.
/// </summary>
public sealed class NpgsqlRlsConnectionInterceptor : DbConnectionInterceptor
{
    private const string CurrentUserKey = "app.current_user_id";
    private const string UserRoleKey = "app.user_role";
    private const string CurrentSiteKey = "app.current_site_id";

    private readonly ISiteRlsContextAccessor _contextAccessor;

    public NpgsqlRlsConnectionInterceptor(ISiteRlsContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        await ApplyAsync(connection, cancellationToken);
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    private async Task ApplyAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        if (connection is not NpgsqlConnection npgsqlConnection)
        {
            return;
        }

        var ctx = _contextAccessor.Current;
        await using var cmd = npgsqlConnection.CreateCommand();
        cmd.CommandText = $@"
            SET ROLE harvestry_app;
            SET row_security = on;
            SELECT
                set_config('{CurrentUserKey}', @userId::text, false),
                set_config('{UserRoleKey}', @role, false),
                set_config('{CurrentSiteKey}', @siteId::text, false);
        ";
        cmd.Parameters.Add("userId", NpgsqlDbType.Uuid).Value = ctx.UserId;
        cmd.Parameters.Add("role", NpgsqlDbType.Text).Value = ctx.Role;
        cmd.Parameters.Add("siteId", NpgsqlDbType.Uuid).Value = ctx.SiteId;

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}

