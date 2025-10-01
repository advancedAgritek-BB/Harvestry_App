using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Harvestry.Identity.Infrastructure.Persistence;
using Harvestry.Shared.Kernel.Serialization;

namespace Harvestry.Identity.Infrastructure.Jobs;

/// <summary>
/// BackgroundService that verifies the authorization_audit hash chain nightly.
/// </summary>
public sealed class AuditChainVerificationJob : BackgroundService
{
    private static readonly TimeSpan VerificationWindow = TimeSpan.FromHours(24);
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuditChainVerificationJob> _logger;

    public AuditChainVerificationJob(IServiceProvider serviceProvider, ILogger<AuditChainVerificationJob> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AuditChainVerificationJob started");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTimeOffset.UtcNow;
            var nextRun = now.Date.AddHours(2); // 02:00 UTC today
            if (nextRun <= now)
            {
                nextRun = nextRun.AddDays(1); // If already past 02:00, schedule for tomorrow
            }
            var delay = nextRun - now;
            if (delay < TimeSpan.Zero)
            {
                delay = TimeSpan.Zero;
            }

            try
            {
                await Task.Delay(delay, stoppingToken);
                await VerifyAsync(stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // graceful shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Audit chain verification failed");
            }
        }
    }

    private async Task VerifyAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var connection = await dbContext.GetOpenConnectionAsync(cancellationToken);

        const string sql = @"
            SELECT audit_id, prev_hash, row_hash, payload, created_at
            FROM authorization_audit
            WHERE created_at >= NOW() - INTERVAL '24 hours'
            ORDER BY created_at ASC;
        ";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        string? previousHash = null;
        var totalRows = 0;
        var mismatches = 0;

        while (await reader.ReadAsync(cancellationToken))
        {
            totalRows++;
            var auditId = reader.GetGuid(reader.GetOrdinal("audit_id"));
            var prevHash = reader.IsDBNull(reader.GetOrdinal("prev_hash")) ? null : reader.GetString(reader.GetOrdinal("prev_hash"));
            var rowHash = reader.IsDBNull(reader.GetOrdinal("row_hash")) ? null : reader.GetString(reader.GetOrdinal("row_hash"));
            var payload = reader.GetString(reader.GetOrdinal("payload"));

            var normalizedPreviousHash = previousHash?.ToUpperInvariant();
            var normalizedPrevHash = prevHash?.ToUpperInvariant();
            if (!string.Equals(normalizedPreviousHash, normalizedPrevHash, StringComparison.Ordinal))
            {
                mismatches++;
                _logger.LogCritical(
                    "Audit chain mismatch at {AuditId}. Expected prev_hash {Expected}, got {Actual}",
                    auditId,
                    previousHash ?? "<null>",
                    prevHash ?? "<null>");
            }

            var computedHash = ComputeRowHash(prevHash, payload);
            var normalizedComputedHash = computedHash?.ToUpperInvariant();
            var normalizedRowHash = rowHash?.ToUpperInvariant();
            if (!string.Equals(normalizedComputedHash, normalizedRowHash, StringComparison.Ordinal))
            {
                mismatches++;
                _logger.LogCritical(
                    "Audit row hash mismatch at {AuditId}. Expected {Expected}, got {Actual}",
                    auditId,
                    computedHash,
                    rowHash ?? "<null>");
            }

            previousHash = rowHash;
        }

        _logger.LogInformation(
            "Audit chain verification completed. Rows checked: {Total}, mismatches: {Mismatches}",
            totalRows,
            mismatches);
    }

    /// <summary>
    /// Computes SHA256 hash of audit row using canonical JSON serialization.
    /// 
    /// Canonical serialization ensures:
    /// - Alphabetical key ordering (deterministic)
    /// - Deterministic output across environments
    /// - No false positives from insertion-order differences
    /// 
    /// Input format: { "payload": "...", "prevHash": "..." }
    /// Keys are automatically sorted alphabetically.
    /// </summary>
    /// <param name="prevHash">Previous hash in the chain</param>
    /// <param name="payload">Audit event payload (JSON string)</param>
    /// <returns>Hex-encoded SHA256 hash</returns>
    private static string ComputeRowHash(string? prevHash, string payload)
    {
        // Use canonical serialization for deterministic hashing
        // Keys will be sorted alphabetically: "payload", "prevHash"
        var hashInput = new Dictionary<string, object>
        {
            { "payload", payload },
            { "prevHash", prevHash ?? string.Empty }
        };

        // Serialize with canonical ordering (keys sorted alphabetically)
        var json = CanonicalJsonSerializer.Serialize(hashInput);

        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(bytes);
    }
}
