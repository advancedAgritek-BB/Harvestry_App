using Harvestry.Sales.Application.Interfaces;
using Harvestry.Sales.Domain.Entities;
using Harvestry.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.Sales.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for Customer aggregate.
/// </summary>
public sealed class CustomerRepository : ICustomerRepository
{
    private readonly SalesDbContext _db;

    public CustomerRepository(SalesDbContext db)
    {
        _db = db;
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<Customer?> GetByLicenseNumberAsync(Guid siteId, string licenseNumber, CancellationToken ct = default)
    {
        return await _db.Customers
            .FirstOrDefaultAsync(c => c.SiteId == siteId && c.LicenseNumber == licenseNumber, ct);
    }

    public async Task<(IReadOnlyList<Customer> Items, int TotalCount)> ListAsync(
        Guid siteId,
        string? search,
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _db.Customers.Where(c => c.SiteId == siteId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(term) ||
                c.LicenseNumber.ToLower().Contains(term) ||
                (c.PrimaryContactName != null && c.PrimaryContactName.ToLower().Contains(term)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<int> GetOrderCountAsync(Guid customerId, CancellationToken ct = default)
    {
        return await _db.SalesOrders.CountAsync(o => o.CustomerId == customerId, ct);
    }

    public async Task AddAsync(Customer customer, CancellationToken ct = default)
    {
        await _db.Customers.AddAsync(customer, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Customer customer, CancellationToken ct = default)
    {
        _db.Customers.Update(customer);
        await _db.SaveChangesAsync(ct);
    }
}
