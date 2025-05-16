using Thrume.Common;

namespace Thrume.Infrastructure;

public static class ExceptionExtensions
{
    public static ProblemDetails ToProblemDetails(this Exception ex, int statusCode) => new ProblemDetails
    {
        Detail = ex.Message,
        Status = statusCode
    };
}