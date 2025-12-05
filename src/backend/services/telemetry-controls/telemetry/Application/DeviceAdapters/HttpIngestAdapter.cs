using System.Linq;
using Harvestry.Telemetry.Application.DTOs;
using Harvestry.Telemetry.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Harvestry.Telemetry.Application.DeviceAdapters;

/// <summary>
/// Handles HTTP-based telemetry ingest requests.
/// </summary>
public sealed class HttpIngestAdapter : IHttpIngestAdapter
{
    private readonly ITelemetryIngestService _ingestService;
    private readonly ITelemetryRlsContextAccessor _rlsContextAccessor;
    private readonly ILogger<HttpIngestAdapter> _logger;

    public HttpIngestAdapter(
        ITelemetryIngestService ingestService,
        ITelemetryRlsContextAccessor rlsContextAccessor,
        ILogger<HttpIngestAdapter> logger)
    {
        _ingestService = ingestService ?? throw new ArgumentNullException(nameof(ingestService));
        _rlsContextAccessor = rlsContextAccessor ?? throw new ArgumentNullException(nameof(rlsContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IngestResultDto> HandleAsync(Guid equipmentId, HttpIngestRequestDto request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.SiteId == Guid.Empty)
        {
            throw new ArgumentException("SiteId is required", nameof(request));
        }

        if (request.Readings == null || request.Readings.Count == 0)
        {
            throw new ArgumentException("At least one reading is required", nameof(request));
        }

        if (request.Readings.Any(r => r.StreamId == Guid.Empty))
        {
            throw new ArgumentException("All readings must specify a valid stream identifier", nameof(request));
        }

        var currentContext = _rlsContextAccessor.Current;
        var contextAdjusted = currentContext.SiteId != request.SiteId;
        if (contextAdjusted)
        {
            _rlsContextAccessor.Set(currentContext with { SiteId = request.SiteId });
        }

        try
        {
            var result = await _ingestService
                .IngestHttpMessageAsync(equipmentId, request.Readings.ToArray(), cancellationToken)
                .ConfigureAwait(false);

            _logger.LogInformation(
                "HTTP ingest completed for SiteId={SiteId}, EquipmentId={EquipmentId}: Accepted={Accepted}, Rejected={Rejected}, Duplicates={Duplicates}",
                request.SiteId,
                equipmentId,
                result.Accepted,
                result.Rejected,
                result.Duplicates);

            return result;
        }
        finally
        {
            if (contextAdjusted)
            {
                _rlsContextAccessor.Set(currentContext);
            }
        }
    }
}
