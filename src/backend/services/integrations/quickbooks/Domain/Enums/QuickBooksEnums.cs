namespace Harvestry.Integration.QuickBooks.Domain.Enums;

/// <summary>
/// Types of entities that can be synchronized with QuickBooks
/// </summary>
public enum QuickBooksEntityType
{
    /// <summary>
    /// Customer/vendor in QuickBooks
    /// </summary>
    Customer = 1,

    /// <summary>
    /// Vendor for purchases
    /// </summary>
    Vendor = 2,

    /// <summary>
    /// Item/product definition
    /// </summary>
    Item = 3,

    /// <summary>
    /// Invoice (sales)
    /// </summary>
    Invoice = 4,

    /// <summary>
    /// Bill (purchases)
    /// </summary>
    Bill = 5,

    /// <summary>
    /// Payment received
    /// </summary>
    Payment = 6,

    /// <summary>
    /// Bill payment
    /// </summary>
    BillPayment = 7,

    /// <summary>
    /// Journal entry
    /// </summary>
    JournalEntry = 8,

    /// <summary>
    /// Purchase order
    /// </summary>
    PurchaseOrder = 9,

    /// <summary>
    /// Sales receipt
    /// </summary>
    SalesReceipt = 10,

    /// <summary>
    /// Inventory adjustment
    /// </summary>
    InventoryAdjustment = 11
}

/// <summary>
/// Status of a QuickBooks sync job
/// </summary>
public enum QuickBooksSyncStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    FailedPermanent = 5,
    Cancelled = 6
}

/// <summary>
/// Types of operations for QuickBooks
/// </summary>
public enum QuickBooksOperationType
{
    Create = 1,
    Update = 2,
    Delete = 3,
    Read = 4
}

/// <summary>
/// QuickBooks connection status
/// </summary>
public enum QuickBooksConnectionStatus
{
    NotConnected = 1,
    Connected = 2,
    TokenExpired = 3,
    RefreshRequired = 4,
    Error = 5
}
