using Harvestry.Transfers.Application.DTOs;

namespace Harvestry.Transfers.Application.Interfaces;

public interface IOutboundTransferService
{
    Task<OutboundTransferDto?> GetByIdAsync(Guid siteId, Guid transferId, CancellationToken cancellationToken = default);
    Task<OutboundTransferListResponse> GetBySiteAsync(Guid siteId, int page = 1, int pageSize = 50, string? status = null, CancellationToken cancellationToken = default);

    Task<OutboundTransferDto> CreateFromShipmentAsync(Guid siteId, CreateOutboundTransferFromShipmentRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<OutboundTransferDto?> MarkReadyAsync(Guid siteId, Guid transferId, Guid userId, CancellationToken cancellationToken = default);
    Task<OutboundTransferDto?> SubmitToMetrcAsync(Guid siteId, Guid transferId, SubmitOutboundTransferToMetrcRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<OutboundTransferDto?> VoidAsync(Guid siteId, Guid transferId, VoidOutboundTransferRequest request, Guid userId, CancellationToken cancellationToken = default);
}

