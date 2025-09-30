using System.Threading;
using System.Threading.Tasks;
using Harvestry.Identity.Domain.Entities;

namespace Harvestry.Identity.Application.Interfaces;

public interface IAuthorizationAuditRepository
{
    Task LogAsync(AuthorizationAuditEntry entry, CancellationToken cancellationToken = default);
}
