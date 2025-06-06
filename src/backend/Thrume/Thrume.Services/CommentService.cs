using Microsoft.EntityFrameworkCore;
using Thrume.Common;
using Thrume.Database;
using Thrume.Domain.DTOs; // DTOs need to be created
using Thrume.Domain.Entity;
using Thrume.Domain.EntityIds;

namespace Thrume.Services;

public class CommentService
{
    private readonly AppDbContext _dbContext;

    public CommentService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CreateCommentAsync(PostId postId, AccountId authorId, string content, CommentId? parentCommentId)
    {
        
        if (string.IsNullOrWhiteSpace(content) || content.Length > 200) 
        {
            return;
        }


        var postExists = await _dbContext.PostDbSet.AnyAsync(p => p.Id == postId);
        if (!postExists)
        {
            return;
        }


        if (parentCommentId.HasValue)
        {
            var parentExists = await _dbContext.CommentDbSet
                .AnyAsync(c => c.Id == parentCommentId.Value && c.PostId == postId);
            if (!parentExists)
            {
                return;
            }
        }


        var comment = new Comment
        {
            Content = content,
            AuthorId = authorId,
            PostId = postId,
            ParentCommentId = parentCommentId,
        };
        
        _dbContext.CommentDbSet.Add(comment);
        await _dbContext.SaveChangesAsync();
    }
    

    public async Task DeleteCommentAsync(CommentId commentId, AccountId userId)
    {

        var comment = await _dbContext.CommentDbSet.FindAsync(commentId);

        if (comment is null)
        {
            return;
        }


        if (comment.AuthorId != userId)
        {
            return;
        }

        _dbContext.CommentDbSet.Remove(comment);
        await _dbContext.SaveChangesAsync();
        
    }
}
