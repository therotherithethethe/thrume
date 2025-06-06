using Thrume.Domain.Entity;
using Thrume.Domain.EntityIds;

namespace Thrume.Api.Endpoints;

public class PostDto
{
    public PostId Id { get; set; }
    public string Content { get; set; }
    public List<Image> Images { get; set; } = [];
    public AccountId AuthorId { get; set; } 
    public string AuthorUserName { get; set; } 
    public string AuthorPictureUrl { get; set; } 
    public List<AccountId> LikedBy { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
    public List<CommentDto> Comments { get; set; } = [];
}