using Harvestry.Integration.QuickBooks.Domain.Entities;
using Harvestry.Integration.QuickBooks.Domain.Enums;

namespace Harvestry.Integration.QuickBooks.Application.Interfaces;

/// <summary>
/// Service interface for QuickBooks connection management
/// </summary>
public interface IQuickBooksConnectionService
{
    /// <summary>
    /// Initiates OAuth2 authorization flow
    /// </summary>
    Task<string> GetAuthorizationUrlAsync(
        Guid siteId,
        string redirectUri,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles OAuth2 callback and exchanges code for tokens
    /// </summary>
    Task<QuickBooksConnection> HandleCallbackAsync(
        string code,
        string realmId,
        string state,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes access token if needed
    /// </summary>
    Task<bool> RefreshTokenIfNeededAsync(
        Guid connectionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets connection for a site
    /// </summary>
    Task<QuickBooksConnection?> GetConnectionAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnects QuickBooks
    /// </summary>
    Task<bool> DisconnectAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests the connection
    /// </summary>
    Task<(bool Success, string Message)> TestConnectionAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for QuickBooks item synchronization
/// </summary>
public interface IQuickBooksItemSyncService
{
    /// <summary>
    /// Syncs all items from Harvestry to QuickBooks
    /// </summary>
    Task<ItemSyncResult> SyncItemsToQuickBooksAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Syncs a single item to QuickBooks
    /// </summary>
    Task<ItemSyncResult> SyncItemAsync(
        Guid siteId,
        Guid harvestryItemId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets item mapping between Harvestry and QuickBooks
    /// </summary>
    Task<IReadOnlyList<ItemMapping>> GetItemMappingsAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for QuickBooks transaction synchronization
/// </summary>
public interface IQuickBooksTransactionService
{
    /// <summary>
    /// Creates an invoice in QuickBooks from a Harvestry sale
    /// </summary>
    Task<TransactionSyncResult> CreateInvoiceAsync(
        Guid siteId,
        Guid salesOrderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a bill in QuickBooks from a Harvestry purchase
    /// </summary>
    Task<TransactionSyncResult> CreateBillAsync(
        Guid siteId,
        Guid purchaseOrderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records inventory adjustment in QuickBooks
    /// </summary>
    Task<TransactionSyncResult> RecordInventoryAdjustmentAsync(
        Guid siteId,
        Guid adjustmentId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of item synchronization
/// </summary>
public sealed record ItemSyncResult
{
    public bool Success { get; init; }
    public int ItemsSynced { get; init; }
    public int ItemsFailed { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
    public DateTimeOffset SyncedAt { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Result of transaction synchronization
/// </summary>
public sealed record TransactionSyncResult
{
    public bool Success { get; init; }
    public string? QuickBooksId { get; init; }
    public string? QuickBooksDocNumber { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTimeOffset SyncedAt { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Mapping between Harvestry item and QuickBooks item
/// </summary>
public sealed record ItemMapping
{
    public Guid HarvestryItemId { get; init; }
    public string? QuickBooksItemId { get; init; }
    public string ItemName { get; init; } = string.Empty;
    public DateTimeOffset? LastSyncedAt { get; init; }
    public bool IsSynced => !string.IsNullOrEmpty(QuickBooksItemId);
}
