using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Harvestry.Identity.API.Middleware;

/// <summary>
/// Translates unhandled exceptions into RFC 7807 problem details responses and logs the failure.
/// </summary>
public sealed class ErrorHandlingMiddleware
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (ValidationException ex)
        {
            await WriteValidationProblemAsync(context, ex).ConfigureAwait(false);
        }
        catch (BadHttpRequestException ex)
        {
        await WriteProblemAsync(
            context,
            statusCode: StatusCodes.Status400BadRequest,
            title: "Invalid request",
            detail: ex.Message,
            traceId: context.TraceIdentifier).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await HandleUnhandledExceptionAsync(context, ex).ConfigureAwait(false);
        }
    }

    private async Task HandleUnhandledExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        _logger.LogError(exception, "Unhandled exception while processing request {TraceIdentifier}", traceId);

        await WriteProblemAsync(
            context,
            statusCode: StatusCodes.Status500InternalServerError,
            title: "Unexpected error",
            detail: "An unexpected error occurred. Refer to the provided trace identifier when contacting support.",
            traceId: traceId).ConfigureAwait(false);
    }

    private static async Task WriteValidationProblemAsync(HttpContext context, ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());

        var traceId = context.TraceIdentifier;
        var problemDetails = new ValidationProblemDetails(errors)
        {
            Title = "Validation failed",
            Detail = "One or more validation errors occurred.",
            Status = StatusCodes.Status400BadRequest,
            Instance = context.Request.Path,
        };

        problemDetails.Extensions["traceId"] = traceId;

        await WriteProblemAsync(context, problemDetails.Status.Value, problemDetails.Title, problemDetails.Detail, problemDetails, traceId);
    }

    private static async Task WriteProblemAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail,
        ProblemDetails? problemDetails = null,
        string? traceId = null)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        problemDetails ??= new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
        };

        if (!problemDetails.Extensions.ContainsKey("traceId") && !string.IsNullOrWhiteSpace(traceId))
        {
            problemDetails.Extensions["traceId"] = traceId;
        }

        await JsonSerializer.SerializeAsync(context.Response.Body, problemDetails, SerializerOptions).ConfigureAwait(false);
    }
}
