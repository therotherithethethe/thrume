using System.Collections.Immutable;
using Microsoft.AspNetCore.Identity;
using Thrume.Domain.EntityIds;

namespace Thrume.Domain.Entity;

public sealed class Account : IdentityUser<AccountId>
{
    public override AccountId Id { get; set; } = Guid.CreateVersion7();
    public override string? ConcurrencyStamp { get; set; } = Guid.CreateVersion7().ToString();
    public List<Post> Posts { get; init; } = [];
    public List<Post> LikedPosts { get; init; } = [];
    public string? PictureUrl { get; init; }
    public List<Subscription> Following { get; init; } = [];

    public List<Subscription> Followers { get; init; } = [];
}