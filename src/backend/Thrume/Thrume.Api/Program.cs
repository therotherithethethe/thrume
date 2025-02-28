using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Thrume.Database;
using Thrume.Domain;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services
    .AddIdentityApiEndpoints<Account>()
    .AddEntityFrameworkStores<AppDbContext>();
builder.Services.AddDbContext<AppDbContext>(o =>
{
    o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

app.MapOpenApi();

if(app.Environment.IsDevelopment()) {
    app.MapScalarApiReference();
}

app.UseAuthorization();

app.Run();

