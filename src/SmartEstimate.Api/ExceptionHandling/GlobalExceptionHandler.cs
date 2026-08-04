using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SmartEstimate.Api.ExceptionHandling;

/// <summary>
/// Converts unhandled exceptions into safe RFC 7807 responses and records a trace identifier.
/// </summary>
public sealed partial class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        LogUnhandledException(logger, exception, httpContext.TraceIdentifier);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Detail = environment.IsDevelopment() ? exception.Message : null,
            Instance = httpContext.Request.Path,
            Type = "https://www.rfc-editor.org/rfc/rfc9110#name-500-internal-server-error"
        };

        problemDetails.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Error,
        Message = "Unhandled exception. TraceId: {TraceId}")]
    private static partial void LogUnhandledException(ILogger logger, Exception exception, string traceId);
}
