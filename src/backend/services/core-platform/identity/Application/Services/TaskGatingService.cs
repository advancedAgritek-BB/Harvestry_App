using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Identity.Application.DTOs;
using Harvestry.Identity.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Harvestry.Identity.Application.Services;

/// <summary>
/// Service for evaluating task gating requirements (SOP/training prerequisites)
/// </summary>
public sealed class TaskGatingService : ITaskGatingService
{
    private readonly IDatabaseRepository _databaseRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<TaskGatingService> _logger;

    public TaskGatingService(
        IDatabaseRepository databaseRepository,
        IUserRepository userRepository,
        ILogger<TaskGatingService> logger)
    {
        _databaseRepository = databaseRepository ?? throw new ArgumentNullException(nameof(databaseRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Check if a user can perform a specific task type
    /// </summary>
    public async Task<TaskGatingResult> CheckTaskGatingAsync(
        Guid userId,
        string taskType,
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(taskType))
            throw new ArgumentException("Task type is required", nameof(taskType));

        _logger.LogInformation(
            "Checking task gating for user {UserId} to perform task type '{TaskType}' at site {SiteId}",
            userId, taskType, siteId);

        try
        {
            // Verify user exists
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found", userId);
                return TaskGatingResult.Block(new List<TaskGatingRequirement>
                {
                    new TaskGatingRequirement("user", userId, "User not found")
                });
            }

            // Call database task gating function
            var (isAllowed, missingRequirements) = await _databaseRepository.CheckTaskGatingAsync(
                userId,
                taskType,
                siteId,
                cancellationToken);

            if (isAllowed)
            {
                _logger.LogInformation(
                    "Task gating passed: User {UserId} can perform task type '{TaskType}'",
                    userId, taskType);

                return TaskGatingResult.Allow();
            }

            _logger.LogWarning(
                "Task gating blocked: User {UserId} cannot perform task type '{TaskType}'. Missing {Count} requirements",
                userId, taskType, missingRequirements.Count);

            // Log each missing requirement for debugging
            foreach (var requirement in missingRequirements)
            {
                _logger.LogWarning(
                    "Missing requirement: Type={Type}, ID={Id}, Reason={Reason}",
                    requirement.RequirementType, requirement.RequirementId, requirement.Reason);
            }

            return TaskGatingResult.Block(missingRequirements);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error checking task gating for user {UserId} to perform task type '{TaskType}'",
                userId, taskType);

            throw;
        }
    }

    /// <summary>
    /// Get all gating requirements for a task type
    /// </summary>
    public async Task<IEnumerable<TaskGatingRequirement>> GetRequirementsForTaskTypeAsync(
        string taskType,
        Guid? siteId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(taskType))
            throw new ArgumentException("Task type is required", nameof(taskType));

        _logger.LogInformation(
            "Fetching gating requirements for task type '{TaskType}' at site {SiteId}",
            taskType, siteId);

        try
        {
            var requirements = await _databaseRepository.GetTaskGatingRequirementsAsync(
                taskType,
                siteId,
                cancellationToken);

            _logger.LogInformation(
                "Found {Count} requirements for task type '{TaskType}'",
                requirements.Count(), taskType);

            return requirements;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error fetching requirements for task type '{TaskType}'",
                taskType);

            throw;
        }
    }
}
