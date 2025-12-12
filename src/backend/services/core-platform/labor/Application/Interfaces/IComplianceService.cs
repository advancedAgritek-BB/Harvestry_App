namespace Harvestry.Labor.Application.Interfaces;

public interface IComplianceService
{
    Task<bool> ValidateShiftAsync(Guid shiftAssignmentId, CancellationToken ct);
    Task<bool> ValidateClockInAsync(Guid employeeId, DateTime clockInUtc, CancellationToken ct);
    Task<bool> IsCertificationValidAsync(Guid employeeId, string certificationName, CancellationToken ct);
}



