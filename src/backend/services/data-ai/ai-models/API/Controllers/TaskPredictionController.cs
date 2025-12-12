using Harvestry.AiModels.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.AiModels.API.Controllers;

/// <summary>
/// API controller for task predictions including assignee recommendations and ETA.
/// </summary>
[ApiController]
[Route("api/v1/ai/tasks")]
[Authorize]
public sealed class TaskPredictionController : ControllerBase
{
    private readonly ITaskPredictionService _taskPrediction;
    private readonly ILogger<TaskPredictionController> _logger;

    public TaskPredictionController(
        ITaskPredictionService taskPrediction,
        ILogger<TaskPredictionController> logger)
    {
        _taskPrediction = taskPrediction;
        _logger = logger;
    }

    /// <summary>
    /// Get recommended assignee for a task
    /// </summary>
    [HttpGet("{taskId}/recommended-assignee")]
    [ProducesResponseType(typeof(AssigneeRecommendationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRecommendedAssignee(
        [FromRoute] Guid taskId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var recommendation = await _taskPrediction.PredictAssigneeAsync(taskId, cancellationToken);

            return Ok(new AssigneeRecommendationDto
            {
                TaskId = recommendation.TaskId,
                RecommendedUserId = recommendation.RecommendedUserId,
                RecommendedUserName = recommendation.RecommendedUserName,
                Confidence = recommendation.Confidence,
                ConfidenceLevel = GetConfidenceLevel(recommendation.Confidence),
                Reasoning = recommendation.Reasoning,
                Alternatives = recommendation.Alternatives.Select(a => new AlternateAssigneeDto
                {
                    UserId = a.UserId,
                    UserName = a.UserName,
                    Confidence = a.Confidence,
                    Reasoning = a.Reasoning
                }).ToList()
            });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = "Task not found in graph" });
        }
    }

    /// <summary>
    /// Get ETA prediction for a task
    /// </summary>
    [HttpGet("{taskId}/eta")]
    [ProducesResponseType(typeof(EtaPredictionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEtaPrediction(
        [FromRoute] Guid taskId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var eta = await _taskPrediction.PredictEtaAsync(taskId, cancellationToken);

            return Ok(new EtaPredictionDto
            {
                TaskId = eta.TaskId,
                PredictedCompletionAt = eta.PredictedCompletionAt,
                PredictedDurationMinutes = (int)eta.PredictedDuration.TotalMinutes,
                PredictedDurationDisplay = FormatDuration(eta.PredictedDuration),
                Confidence = eta.Confidence,
                ConfidenceLevel = GetConfidenceLevel(eta.Confidence),
                ConfidenceIntervalLowMinutes = (int)eta.ConfidenceIntervalLow.TotalMinutes,
                ConfidenceIntervalHighMinutes = (int)eta.ConfidenceIntervalHigh.TotalMinutes,
                RiskFactors = eta.RiskFactors.ToList()
            });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = "Task not found in graph" });
        }
    }

    /// <summary>
    /// Get critical path tasks for a site
    /// </summary>
    [HttpGet("site/{siteId}/critical-path")]
    [ProducesResponseType(typeof(CriticalPathResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCriticalPath(
        [FromRoute] Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var criticalTasks = await _taskPrediction.FindCriticalPathAsync(siteId, cancellationToken);

        return Ok(new CriticalPathResponse
        {
            SiteId = siteId,
            CriticalTasks = criticalTasks.Select(t => new CriticalPathTaskDto
            {
                TaskId = t.TaskId,
                Title = t.Title,
                DependentTaskCount = t.DependentTaskCount,
                TotalBlockedTimeMinutes = (int)t.TotalBlockedTime.TotalMinutes,
                TotalBlockedTimeDisplay = FormatDuration(t.TotalBlockedTime),
                ImpactScore = t.ImpactScore,
                ImpactLevel = GetImpactLevel(t.ImpactScore)
            }).ToList(),
            TotalCriticalTasks = criticalTasks.Count
        });
    }

    /// <summary>
    /// Batch predict for multiple tasks
    /// </summary>
    [HttpPost("batch-predict")]
    [ProducesResponseType(typeof(BatchPredictionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> BatchPredict(
        [FromBody] BatchPredictionRequest request,
        CancellationToken cancellationToken = default)
    {
        var predictions = new List<TaskPredictionDto>();

        foreach (var taskId in request.TaskIds)
        {
            try
            {
                var assignee = request.IncludeAssignee
                    ? await _taskPrediction.PredictAssigneeAsync(taskId, cancellationToken)
                    : null;

                var eta = request.IncludeEta
                    ? await _taskPrediction.PredictEtaAsync(taskId, cancellationToken)
                    : null;

                predictions.Add(new TaskPredictionDto
                {
                    TaskId = taskId,
                    Assignee = assignee != null ? new AssigneeRecommendationDto
                    {
                        TaskId = assignee.TaskId,
                        RecommendedUserId = assignee.RecommendedUserId,
                        RecommendedUserName = assignee.RecommendedUserName,
                        Confidence = assignee.Confidence,
                        ConfidenceLevel = GetConfidenceLevel(assignee.Confidence),
                        Reasoning = assignee.Reasoning,
                        Alternatives = assignee.Alternatives.Select(a => new AlternateAssigneeDto
                        {
                            UserId = a.UserId,
                            UserName = a.UserName,
                            Confidence = a.Confidence,
                            Reasoning = a.Reasoning
                        }).ToList()
                    } : null,
                    Eta = eta != null ? new EtaPredictionDto
                    {
                        TaskId = eta.TaskId,
                        PredictedCompletionAt = eta.PredictedCompletionAt,
                        PredictedDurationMinutes = (int)eta.PredictedDuration.TotalMinutes,
                        PredictedDurationDisplay = FormatDuration(eta.PredictedDuration),
                        Confidence = eta.Confidence,
                        ConfidenceLevel = GetConfidenceLevel(eta.Confidence),
                        ConfidenceIntervalLowMinutes = (int)eta.ConfidenceIntervalLow.TotalMinutes,
                        ConfidenceIntervalHighMinutes = (int)eta.ConfidenceIntervalHigh.TotalMinutes,
                        RiskFactors = eta.RiskFactors.ToList()
                    } : null
                });
            }
            catch (InvalidOperationException)
            {
                // Skip tasks not found in graph
                _logger.LogDebug("Task {TaskId} not found in graph, skipping", taskId);
            }
        }

        return Ok(new BatchPredictionResponse
        {
            Predictions = predictions,
            ProcessedCount = predictions.Count,
            RequestedCount = request.TaskIds.Count
        });
    }

    private static string GetConfidenceLevel(float confidence)
    {
        return confidence switch
        {
            >= 0.8f => "High",
            >= 0.5f => "Medium",
            _ => "Low"
        };
    }

    private static string GetImpactLevel(float impactScore)
    {
        return impactScore switch
        {
            >= 0.7f => "Critical",
            >= 0.4f => "High",
            >= 0.2f => "Medium",
            _ => "Low"
        };
    }

    private static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalHours >= 24)
        {
            var days = (int)duration.TotalDays;
            var hours = duration.Hours;
            return hours > 0 ? $"{days}d {hours}h" : $"{days}d";
        }
        else if (duration.TotalMinutes >= 60)
        {
            var hours = (int)duration.TotalHours;
            var minutes = duration.Minutes;
            return minutes > 0 ? $"{hours}h {minutes}m" : $"{hours}h";
        }
        else
        {
            return $"{(int)duration.TotalMinutes}m";
        }
    }
}

#region DTOs

public sealed class AssigneeRecommendationDto
{
    public Guid TaskId { get; init; }
    public Guid RecommendedUserId { get; init; }
    public string RecommendedUserName { get; init; } = string.Empty;
    public float Confidence { get; init; }
    public string ConfidenceLevel { get; init; } = string.Empty;
    public string Reasoning { get; init; } = string.Empty;
    public IReadOnlyList<AlternateAssigneeDto> Alternatives { get; init; } 
        = Array.Empty<AlternateAssigneeDto>();
}

public sealed class AlternateAssigneeDto
{
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public float Confidence { get; init; }
    public string Reasoning { get; init; } = string.Empty;
}

public sealed class EtaPredictionDto
{
    public Guid TaskId { get; init; }
    public DateTime PredictedCompletionAt { get; init; }
    public int PredictedDurationMinutes { get; init; }
    public string PredictedDurationDisplay { get; init; } = string.Empty;
    public float Confidence { get; init; }
    public string ConfidenceLevel { get; init; } = string.Empty;
    public int ConfidenceIntervalLowMinutes { get; init; }
    public int ConfidenceIntervalHighMinutes { get; init; }
    public IReadOnlyList<string> RiskFactors { get; init; } = Array.Empty<string>();
}

public sealed class CriticalPathResponse
{
    public Guid SiteId { get; init; }
    public IReadOnlyList<CriticalPathTaskDto> CriticalTasks { get; init; } 
        = Array.Empty<CriticalPathTaskDto>();
    public int TotalCriticalTasks { get; init; }
}

public sealed class CriticalPathTaskDto
{
    public Guid TaskId { get; init; }
    public string Title { get; init; } = string.Empty;
    public int DependentTaskCount { get; init; }
    public int TotalBlockedTimeMinutes { get; init; }
    public string TotalBlockedTimeDisplay { get; init; } = string.Empty;
    public float ImpactScore { get; init; }
    public string ImpactLevel { get; init; } = string.Empty;
}

public sealed class BatchPredictionRequest
{
    public IReadOnlyList<Guid> TaskIds { get; init; } = Array.Empty<Guid>();
    public bool IncludeAssignee { get; init; } = true;
    public bool IncludeEta { get; init; } = true;
}

public sealed class BatchPredictionResponse
{
    public IReadOnlyList<TaskPredictionDto> Predictions { get; init; } 
        = Array.Empty<TaskPredictionDto>();
    public int ProcessedCount { get; init; }
    public int RequestedCount { get; init; }
}

public sealed class TaskPredictionDto
{
    public Guid TaskId { get; init; }
    public AssigneeRecommendationDto? Assignee { get; init; }
    public EtaPredictionDto? Eta { get; init; }
}

#endregion
