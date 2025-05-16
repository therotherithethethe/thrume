using Thrume.Domain.EntityIds;

namespace Thrume.Domain.Entity;

public sealed class Post
{
    public PostId Id { get; init; } = Guid.CreateVersion7();
    public string Content { get; init; }
    public List<Image> Images = [];
    public Account Author { get; init; }
    public List<Account> LikedBy { get; init; } = [];
    public List<Comment> Comments { get; init; } = []; 
    public DateTimeOffset CreatedAt { get; init; } = DateTime.UtcNow;
}