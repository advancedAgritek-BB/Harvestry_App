using Harvestry.Identity.Application.Interfaces;
using Harvestry.Identity.Domain.Entities;
using Harvestry.Shared.Kernel.Domain;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.Identity.Infrastructure.Persistence;

public class SiteRepository : ISiteRepository
{
    private readonly IdentityDbContext _context;

    public SiteRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Site?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sites.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Site>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Sites.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Site>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserSites
            .Where(us => us.UserId == userId)
            .Select(us => us.Site)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Sites.AnyAsync(s => s.Name == name, cancellationToken);
    }

    public async Task AddAsync(Site site, CancellationToken cancellationToken = default)
    {
        await _context.Sites.AddAsync(site, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Site site, CancellationToken cancellationToken = default)
    {
        _context.Sites.Update(site);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Site site, CancellationToken cancellationToken = default)
    {
        _context.Sites.Remove(site);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
