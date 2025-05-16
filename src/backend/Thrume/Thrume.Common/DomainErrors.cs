using Microsoft.AspNetCore.Http;
using Thrume.Domain.EntityIds;

namespace Thrume.Common;

public static class DomainErrors
{
    public static ProblemDetails ImageToLarge => new()
    {
        Detail = "File size is large than 10 MB.",
        Status = StatusCodes.Status413RequestEntityTooLarge
    };

    public static ProblemDetails InvalidImageType => new()
    {
        Detail = "Image type must be .jpg/jpeg/png",
        Status = StatusCodes.Status409Conflict
    };
    public static ProblemDetails AccountNotFound(AccountId id) => new()
    {
        Detail = $"Account with id {id} is not found.",
        Status = StatusCodes.Status409Conflict
    };
}