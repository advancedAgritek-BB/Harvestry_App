using Harvestry.Sales.Application.DTOs;
using Harvestry.Sales.Application.Interfaces;
using Harvestry.Sales.Domain.Entities;
using Harvestry.Sales.Domain.Enums;

namespace Harvestry.Sales.Application.Services;

/// <summary>
/// Application service for Customer operations.
/// </summary>
public sealed class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<CustomerDetailDto> GetByIdAsync(Guid customerId, CancellationToken ct = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, ct)
            ?? throw new KeyNotFoundException($"Customer {customerId} not found");

        return MapToDetail(customer);
    }

    public async Task<CustomerListResponse> ListAsync(
        Guid siteId,
        string? search,
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var (items, totalCount) = await _customerRepository.ListAsync(
            siteId, search, isActive, page, pageSize, ct);

        var summaries = new List<CustomerSummaryDto>(items.Count);
        foreach (var customer in items)
        {
            var orderCount = await _customerRepository.GetOrderCountAsync(customer.Id, ct);
            summaries.Add(MapToSummary(customer, orderCount));
        }

        return new CustomerListResponse(summaries, totalCount, page, pageSize);
    }

    public async Task<CustomerDetailDto> CreateAsync(
        Guid siteId,
        CreateCustomerRequest request,
        Guid userId,
        CancellationToken ct = default)
    {
        // Check for duplicate license number
        var existing = await _customerRepository.GetByLicenseNumberAsync(siteId, request.LicenseNumber, ct);
        if (existing != null)
        {
            throw new InvalidOperationException($"A customer with license number '{request.LicenseNumber}' already exists.");
        }

        var customer = Customer.Create(siteId, request.Name, request.LicenseNumber, userId);

        if (!string.IsNullOrWhiteSpace(request.FacilityName) || !string.IsNullOrWhiteSpace(request.FacilityType))
        {
            customer.UpdateBusinessInfo(request.Name, request.LicenseNumber, request.FacilityName, request.FacilityType, userId);
        }

        if (!string.IsNullOrWhiteSpace(request.Address) || !string.IsNullOrWhiteSpace(request.City))
        {
            customer.UpdateAddress(request.Address, request.City, request.State, request.Zip, userId);
        }

        if (!string.IsNullOrWhiteSpace(request.PrimaryContactName) || !string.IsNullOrWhiteSpace(request.Email))
        {
            customer.UpdateContact(request.PrimaryContactName, request.Email, request.Phone, userId);
        }

        if (!string.IsNullOrWhiteSpace(request.MetrcRecipientId))
        {
            customer.SetMetrcRecipientId(request.MetrcRecipientId, userId);
        }

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            customer.SetNotes(request.Notes, userId);
        }

        if (!string.IsNullOrWhiteSpace(request.Tags))
        {
            customer.SetTags(request.Tags, userId);
        }

        await _customerRepository.AddAsync(customer, ct);
        return MapToDetail(customer);
    }

    public async Task<CustomerDetailDto> UpdateAsync(
        Guid customerId,
        UpdateCustomerRequest request,
        Guid userId,
        CancellationToken ct = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, ct)
            ?? throw new KeyNotFoundException($"Customer {customerId} not found");

        customer.UpdateBusinessInfo(request.Name, request.LicenseNumber, request.FacilityName, request.FacilityType, userId);
        customer.UpdateAddress(request.Address, request.City, request.State, request.Zip, userId);
        customer.UpdateContact(request.PrimaryContactName, request.Email, request.Phone, userId);
        customer.SetMetrcRecipientId(request.MetrcRecipientId, userId);
        customer.SetNotes(request.Notes, userId);
        customer.SetTags(request.Tags, userId);

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value)
                customer.Activate(userId);
            else
                customer.Deactivate(userId);
        }

        await _customerRepository.UpdateAsync(customer, ct);
        return MapToDetail(customer);
    }

    public async Task<CustomerDetailDto> UpdateLicenseVerificationAsync(
        Guid customerId,
        UpdateLicenseVerificationRequest request,
        Guid userId,
        CancellationToken ct = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, ct)
            ?? throw new KeyNotFoundException($"Customer {customerId} not found");

        var status = Enum.Parse<LicenseVerificationStatus>(request.Status, ignoreCase: true);
        customer.SetLicenseVerificationStatus(status, request.Source, request.Notes, userId);

        await _customerRepository.UpdateAsync(customer, ct);
        return MapToDetail(customer);
    }

    private static CustomerDetailDto MapToDetail(Customer c) => new(
        c.Id,
        c.SiteId,
        c.Name,
        c.LicenseNumber,
        c.FacilityName,
        c.FacilityType,
        c.Address,
        c.City,
        c.State,
        c.Zip,
        c.PrimaryContactName,
        c.Email,
        c.Phone,
        c.LicenseVerifiedStatus.ToString(),
        c.LicenseVerifiedAt,
        c.LicenseVerificationSource,
        c.LicenseVerificationNotes,
        c.MetrcRecipientId,
        c.IsActive,
        c.Notes,
        c.Tags,
        c.CreatedAt,
        c.UpdatedAt
    );

    private static CustomerSummaryDto MapToSummary(Customer c, int orderCount) => new(
        c.Id,
        c.SiteId,
        c.Name,
        c.LicenseNumber,
        c.FacilityName,
        c.FacilityType,
        c.PrimaryContactName,
        c.Email,
        c.Phone,
        c.LicenseVerifiedStatus.ToString(),
        orderCount,
        c.IsActive
    );
}
