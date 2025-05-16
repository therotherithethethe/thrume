using Thrume.Domain.EntityIds;

namespace Thrume.Domain.Entity;

public sealed class Comment
{
    public CommentId Id { get; init; } = Guid.CreateVersion7();
    public string Content { get; set; } = string.Empty; 


    public AccountId AuthorId { get; init; }
    public Account Author { get; init; } = null!;

    public PostId PostId { get; init; }
    public Post Post { get; init; } = null!;

    public CommentId? ParentCommentId { get; init; }
    public Comment? ParentComment { get; init; }
    public List<Comment> Replies { get; init; } = []; 


    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}