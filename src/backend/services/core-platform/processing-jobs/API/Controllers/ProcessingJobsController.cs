using Harvestry.ProcessingJobs.Application.DTOs;
using Harvestry.ProcessingJobs.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.ProcessingJobs.API.Controllers;

/// <summary>
/// API controller for Processing Job management
/// </summary>
[ApiController]
[Route("api/v1/sites/{siteId:guid}/processing-jobs")]
[Authorize]
public class ProcessingJobsController : ControllerBase
{
    private readonly IProcessingJobService _service;
    private readonly ILogger<ProcessingJobsController> _logger;

    public ProcessingJobsController(IProcessingJobService service, ILogger<ProcessingJobsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ProcessingJobListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProcessingJobListResponse>> GetJobs(
        [FromRoute] Guid siteId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50,
        [FromQuery] string? status = null, [FromQuery] Guid? typeId = null,
        [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetJobsAsync(siteId, page, pageSize, status, typeId, fromDate, toDate, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProcessingJobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProcessingJobDto>> GetJob([FromRoute] Guid siteId, [FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var job = await _service.GetByIdAsync(siteId, id, cancellationToken);
        return job == null ? NotFound() : Ok(job);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProcessingJobDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProcessingJobDto>> CreateJob([FromRoute] Guid siteId, [FromBody] CreateProcessingJobRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var job = await _service.CreateAsync(siteId, request, GetCurrentUserId(), cancellationToken);
            return CreatedAtAction(nameof(GetJob), new { siteId, id = job.Id }, job);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/inputs")]
    [ProducesResponseType(typeof(ProcessingJobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProcessingJobDto>> AddInput([FromRoute] Guid siteId, [FromRoute] Guid id, [FromBody] AddInputRequest request, CancellationToken cancellationToken = default)
    {
        var job = await _service.AddInputAsync(siteId, id, request, GetCurrentUserId(), cancellationToken);
        return job == null ? NotFound() : Ok(job);
    }

    [HttpPost("{id:guid}/outputs")]
    [ProducesResponseType(typeof(ProcessingJobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProcessingJobDto>> AddOutput([FromRoute] Guid siteId, [FromRoute] Guid id, [FromBody] AddOutputRequest request, CancellationToken cancellationToken = default)
    {
        var job = await _service.AddOutputAsync(siteId, id, request, GetCurrentUserId(), cancellationToken);
        return job == null ? NotFound() : Ok(job);
    }

    [HttpPost("{id:guid}/finish")]
    [ProducesResponseType(typeof(ProcessingJobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProcessingJobDto>> FinishJob([FromRoute] Guid siteId, [FromRoute] Guid id, [FromBody] FinishJobRequest request, CancellationToken cancellationToken = default)
    {
        var job = await _service.FinishAsync(siteId, id, request, GetCurrentUserId(), cancellationToken);
        return job == null ? NotFound() : Ok(job);
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ProcessingJobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProcessingJobDto>> CancelJob([FromRoute] Guid siteId, [FromRoute] Guid id, [FromBody] string reason, CancellationToken cancellationToken = default)
    {
        var job = await _service.CancelAsync(siteId, id, reason, GetCurrentUserId(), cancellationToken);
        return job == null ? NotFound() : Ok(job);
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst("sub") ?? User.FindFirst("user_id");
        return claim != null && Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty;
    }
}

/// <summary>
/// API controller for Processing Job Types
/// </summary>
[ApiController]
[Route("api/v1/sites/{siteId:guid}/processing-job-types")]
[Authorize]
public class ProcessingJobTypesController : ControllerBase
{
    private readonly IProcessingJobService _service;

    public ProcessingJobTypesController(IProcessingJobService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(List<ProcessingJobTypeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProcessingJobTypeDto>>> GetTypes([FromRoute] Guid siteId, CancellationToken cancellationToken = default)
    {
        var types = await _service.GetJobTypesAsync(siteId, cancellationToken);
        return Ok(types);
    }
}




