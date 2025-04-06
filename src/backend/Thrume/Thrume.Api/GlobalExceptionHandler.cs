using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Office.Interop.Outlook;
using Thrume.Services.Exceptions;
using Exception = System.Exception;

namespace Thrume.Api;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Exception occured: {Message}", exception.Message);
        var problemDetails = Map(exception);
    
    }

    private ProblemDetails Map(Exception exception)
    {
        return exception switch
        {
            FileToLargeException ex => new ProblemDetails
            {
                Detail = ex.Message,
                Title = "File to large.",
                Status = StatusCodes.Status413RequestEntityTooLarge
            },
            InvalidFileTypeException ex => new ProblemDetails
            {
                Detail = ex.Message,
                Title = "File to large.",
                Status = StatusCodes.Status413RequestEntityTooLarge
            },
        };
    }
}