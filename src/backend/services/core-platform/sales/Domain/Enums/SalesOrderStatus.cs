namespace Harvestry.Sales.Domain.Enums;

public enum SalesOrderStatus
{
    Draft = 0,
    Submitted = 1,
    Allocated = 2,
    PartiallyShipped = 3,
    Shipped = 4,
    Cancelled = 5
}

