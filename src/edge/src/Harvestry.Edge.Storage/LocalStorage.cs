using System.Data;
using System.Data.Common;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Harvestry.Edge.Storage;

public class LocalStorage
{
    private readonly string _connectionString;

    public LocalStorage(string databasePath = "harvestry-edge.db")
    {
        _connectionString = $"Data Source={databasePath}";
    }

    public async Task InitializeAsync()
    {
        using var conn = GetConnection();
        await conn.OpenAsync();

        // 1. KeyValueStore (Config, State)
        await conn.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS KeyValueStore (
                Key TEXT PRIMARY KEY,
                Value TEXT,
                UpdatedAt TEXT
            )");

        // 2. TelemetryQueue (Offline Buffer)
        await conn.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS TelemetryQueue (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Topic TEXT,
                Payload TEXT,
                CreatedAt TEXT,
                Priority INTEGER DEFAULT 0
            )");

        // 3. Programs (Recipes/Schedules)
        await conn.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS Programs (
                Id TEXT PRIMARY KEY,
                JsonBlob TEXT,
                Version INTEGER,
                IsActive INTEGER DEFAULT 0
            )");
    }

    // Return DbConnection to support OpenAsync
    public DbConnection GetConnection() => new SqliteConnection(_connectionString);

    // --- KeyValue Helpers ---
    public async Task SetConfigAsync(string key, string value)
    {
        using var conn = GetConnection();
        await conn.ExecuteAsync(@"
            INSERT INTO KeyValueStore (Key, Value, UpdatedAt) 
            VALUES (@Key, @Value, @UpdatedAt)
            ON CONFLICT(Key) DO UPDATE SET Value = @Value, UpdatedAt = @UpdatedAt",
            new { Key = key, Value = value, UpdatedAt = DateTime.UtcNow });
    }

    public async Task<string?> GetConfigAsync(string key)
    {
        using var conn = GetConnection();
        return await conn.QuerySingleOrDefaultAsync<string>(
            "SELECT Value FROM KeyValueStore WHERE Key = @Key", new { Key = key });
    }

    // --- Telemetry Helpers ---
    public async Task EnqueueTelemetryAsync(string topic, string payload, int priority = 0)
    {
        using var conn = GetConnection();
        await conn.ExecuteAsync(@"
            INSERT INTO TelemetryQueue (Topic, Payload, CreatedAt, Priority)
            VALUES (@Topic, @Payload, @CreatedAt, @Priority)",
            new { Topic = topic, Payload = payload, CreatedAt = DateTime.UtcNow, Priority = priority });
    }

    public async Task<IEnumerable<TelemetryItem>> DequeueTelemetryAsync(int limit = 50)
    {
        using var conn = GetConnection();
        // Simple FIFO for now, ignoring priority in fetch order but preserving it in struct
        return await conn.QueryAsync<TelemetryItem>(
            "SELECT * FROM TelemetryQueue ORDER BY Id ASC LIMIT @Limit", new { Limit = limit });
    }

    public async Task RemoveTelemetryAsync(IEnumerable<long> ids)
    {
        using var conn = GetConnection();
        await conn.ExecuteAsync(
            "DELETE FROM TelemetryQueue WHERE Id IN @Ids", new { Ids = ids });
    }
}

public class TelemetryItem
{
    public long Id { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int Priority { get; set; }
}





