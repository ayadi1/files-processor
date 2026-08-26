using FilesProcessor.WebApi.Core.Dtos;
using Microsoft.AspNetCore.Diagnostics;

namespace FilesProcessor.WebApi.Core.Exceptions.Handlers;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            FileTooLargeException ex => (StatusCodes.Status413PayloadTooLarge, "File too large"),
            FileNotFoundException => (StatusCodes.Status404NotFound, "File not found"),
            DirectoryNotFoundException => (StatusCodes.Status404NotFound, "File not found"),
            OperationCanceledException => (StatusCodes.Status499ClientClosedRequest, "Request cancelled"),
            _ => (StatusCodes.Status500InternalServerError, "Server error"),
        };

        // don't log at Error for 4xx/cancel — those aren't server faults
        if (status >= 500)
        {
            logger.LogError(exception, "Unhandled exception");
        }
        else
        {
            logger.LogWarning(exception, "Handled exception -> {Status}", status);
        }

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(
            new ApiErrorResponse(status, title, exception.Message, httpContext.TraceIdentifier),
            cancellationToken);

        return true;
    }
}
