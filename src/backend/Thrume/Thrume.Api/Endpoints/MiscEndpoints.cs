// Thrume.Api/Endpoints/MiscEndpoints.cs
using Microsoft.AspNetCore.Authorization;

namespace Thrume.Api.Endpoints;

public static class MiscEndpoints
{
    public static IEndpointRouteBuilder MapMiscEndpoints(this IEndpointRouteBuilder app)
    {
        var mapGroup = app.MapGroup("/test");

        // GET /exception
        mapGroup.MapGet("/exception", () => { throw new InvalidOperationException("Sample Exception"); });

        // GET /secure
        mapGroup.MapGet("/secure", [Authorize]() => "Secure content");

        // GET /unsecure
        mapGroup.MapGet("/unsecure", [AllowAnonymous]() => "Unsecure content"); // Explicitly AllowAnonymous

        return app; // Return the builder to allow chaining
    }
}