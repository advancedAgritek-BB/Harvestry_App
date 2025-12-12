using Harvestry.Sales.Application.DTOs;

namespace Harvestry.Sales.Application.Interfaces;

/// <summary>
/// Application service for Customer operations.
/// </summary>
public interface ICustomerService
{
    Task<CustomerDetailDto> GetByIdAsync(Guid customerId, CancellationToken ct = default);

    Task<CustomerListResponse> ListAsync(
        Guid siteId,
        string? search,
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<CustomerDetailDto> CreateAsync(
        Guid siteId,
        CreateCustomerRequest request,
        Guid userId,
        CancellationToken ct = default);

    Task<CustomerDetailDto> UpdateAsync(
        Guid customerId,
        UpdateCustomerRequest request,
        Guid userId,
        CancellationToken ct = default);

    Task<CustomerDetailDto> UpdateLicenseVerificationAsync(
        Guid customerId,
        UpdateLicenseVerificationRequest request,
        Guid userId,
        CancellationToken ct = default);
}
