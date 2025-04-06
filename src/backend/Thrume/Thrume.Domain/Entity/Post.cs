using Thrume.Domain.EntityIds;

namespace Thrume.Domain.Entity;

public sealed class Post
{
    public PostId Id { get; init; } = Guid.CreateVersion7();
    public string Content { get; init; }
    public Account Author { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTime.UtcNow;
    
    public Post() {} //Ef core

}