using Microsoft.AspNetCore.Authorization;
using Scalar.AspNetCore;
using Thrume.Api;
using Thrume.Domain;

var builder = WebApplication.CreateBuilder(args);

(await builder.ConfigureEmailSender())
    .ConfigureIdentity()
    .ConfigureDatabase()
    .ConfigureAuth();

var app = builder.Build();


app.MapScalarApiReference().WithMetadata(new AllowAnonymousAttribute());
app.MapOpenApi().WithMetadata(new AllowAnonymousAttribute());
app.UseAuthentication();
app.UseCors();
app.UseAuthorization();

app.MapIdentityApi<Account>().WithMetadata(new AllowAnonymousAttribute());
app.MapGet("/secure", [Authorize]() => "Secure content");
app.MapGet("/unsecure", [AllowAnonymous]() => "Secure content");
app.Run();