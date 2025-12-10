using Harvestry.Labor.Domain.Entities;
using Harvestry.Labor.Domain.Enums;

namespace Harvestry.Labor.Application.Interfaces;

public interface IEmployeeService
{
    Task<Employee> CreateAsync(Guid siteId, string fullName, string role, PayType payType, decimal rate, CancellationToken ct);
    Task AssignSkillAsync(Guid employeeId, string skill, CancellationToken ct);
    Task AssignCertificationAsync(Guid employeeId, string certName, DateOnly? expiresOn, CancellationToken ct);
    Task SetStatusAsync(Guid employeeId, EmploymentStatus status, CancellationToken ct);
}


