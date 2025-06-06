using Thrume.Domain.Entity;
using Thrume.Domain.EntityIds;

namespace Thrume.Api.Endpoints;

public class CommentDto
{
    public CommentId Id { get; set; } // Was CommentId
    public string Content { get; set; }
    public AccountId AuthorId { get; set; }
    public PostId PostId { get; set; }
    public CommentId? ParentCommentId { get; set; }
    public CommentDto? ParentComment { get; set; } // Recursive mapping
    public List<CommentDto?> Replies { get; set; } = []; // Recursive mapping
    public DateTimeOffset CreatedAt { get; set; }
    
    public static CommentDto? ToDto(Comment? comment)
    {
        if (comment == null)
        {
            return null;
        }

        return new CommentDto
        {
            Id = comment.Id, 
            Content = comment.Content,
            AuthorId = comment.AuthorId,
            PostId = comment.PostId,
            ParentCommentId = comment.ParentCommentId,
            ParentComment = comment.ParentComment != null ? ToDto(comment.ParentComment) : null,
            Replies = comment.Replies?.Select(ToDto).ToList() ?? [],
            CreatedAt = comment.CreatedAt // Add if CommentDto has CreatedAt
        };
    }
}