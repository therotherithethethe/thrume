using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Thrume.Api;
using Thrume.Api.Extensions;
using Thrume.Database;
using Thrume.Domain.DTOs;
using Thrume.Domain.Entity;
using Thrume.Domain.EntityIds;
using Thrume.Services;
using Thrume.Services.Abstraction;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<ServiceProviderOptions>(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddScoped<IAvatarService, AvatarFileService>();
builder.ConfigureEmailSender()
    .ConfigureIdentity()
    .ConfigureDatabase()
    .ConfigureAuth()
    .ConfigureMinio();

var app = builder.Build();
app.UseExceptionHandler();

app.MapScalarApiReference();
app.MapOpenApi();
app.UseAuthentication();
app.UseCors();
app.UseAuthorization();

app.MapGet("/exception", () => 
{
    throw new InvalidOperationException("Sample Exception");
});


app.MapIdentityApi<Account>();
app.MapGet("/secure", [Authorize]() => "Secure content");
app.MapGet("/unsecure", [AllowAnonymous]() => "Secure content");
app.MapPost("/posts", [Authorize] async (
    HttpContext context,
    AppDbContext dbContext,
    [FromBody] CreatePostRequest createPostRequest) =>
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)!;

        var guid = Guid.Parse(userId.Value);
        var account = await dbContext.AccountDbSet.FindAsync(new AccountId(guid));
        if (account is null) return Results.Unauthorized(); //TODO: idk if this is the best variant
        dbContext.PostDbSet.Add(new Post
        {
            Author = account,
            Content = createPostRequest.Content
        });
        await dbContext.SaveChangesAsync();
        return Results.Ok();
    });
app.MapGet("/posts", [Authorize] async (HttpContext context, AppDbContext dbContext) =>
{
    var user = context.User.FindFirst(ClaimTypes.NameIdentifier)!;
    var guid = Guid.Parse(user.Value);
    var acc = await dbContext.PostDbSet
        .AsNoTracking()
        .Where(p => p.Author.Id == new AccountId(guid))
        .Select(p => p.Content)
        .ToListAsync();
    if (acc is null) return Results.Unauthorized();

    return Results.Ok(acc);
});


app.MapPut("/upload", async (IFormFile file, HttpContext context, IFileStorageService storageService) =>
{
    await using var stream = file.OpenReadStream();
    await storageService.UploadFileAsync(stream, file.FileName, file.ContentType);
}).Accepts<IFormFile>("multipart/form-data").DisableAntiforgery();
app.MapPut("profile/update", async (
    [FromBody] UpdateCurrentAccountRequest request,
    HttpContext context,
    IFileStorageService fileStorageService) =>
{
    await fileStorageService.UploadFileAsync(
        fileStream: request.Picture.OpenReadStream(),
        fileName: context.User.FindFirst(ClaimTypes.NameIdentifier)!.Value,
        contentType: request.Picture.ContentType);
})
.Accepts<IFormFile>("multipart/form-data")
.DisableAntiforgery(); //TODO
app.Run();