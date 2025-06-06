using Thrume.Domain.EntityIds;

namespace Thrume.Api.Endpoints;

public record CreateCommentPayload(PostId PostId, string Content, CommentId? ParentCommentId);