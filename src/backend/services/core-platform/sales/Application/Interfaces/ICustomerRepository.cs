using Harvestry.Sales.Domain.Entities;

namespace Harvestry.Sales.Application.Interfaces;

/// <summary>
/// Repository for Customer aggregate.
/// </summary>
public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default);
    
    Task<Customer?> GetByLicenseNumberAsync(Guid siteId, string licenseNumber, CancellationToken ct = default);
    
    Task<(IReadOnlyList<Customer> Items, int TotalCount)> ListAsync(
        Guid siteId,
        string? search,
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<int> GetOrderCountAsync(Guid customerId, CancellationToken ct = default);

    Task AddAsync(Customer customer, CancellationToken ct = default);
    
    Task UpdateAsync(Customer customer, CancellationToken ct = default);
}
