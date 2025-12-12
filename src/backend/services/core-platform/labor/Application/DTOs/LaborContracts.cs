namespace Harvestry.Labor.Application.DTOs;

public record EmployeeRequest(Guid SiteId, string FullName, string Role, string PayType, decimal Rate);
public record ShiftAssignmentRequest(Guid SiteId, Guid EmployeeId, Guid ShiftTemplateId, DateOnly ShiftDate);
public record TimeEntryRequest(Guid SiteId, Guid EmployeeId, Guid? ShiftAssignmentId, string Source, string? TaskReference);
public record BudgetRequest(Guid SiteId, string Scope, decimal BudgetAmount, DateOnly StartDate, DateOnly EndDate);

public record EmployeeResponse(Guid Id, Guid SiteId, string FullName, string Role, string PayType, decimal Rate, string Status);
public record ShiftAssignmentResponse(Guid Id, Guid EmployeeId, Guid? ShiftTemplateId, string Status, string? RoomCode);
public record TimeEntryResponse(Guid Id, Guid EmployeeId, string Status, decimal? Hours, decimal? Cost);
public record BudgetResponse(Guid Id, string Scope, decimal Budget, decimal Actual, decimal Variance);



