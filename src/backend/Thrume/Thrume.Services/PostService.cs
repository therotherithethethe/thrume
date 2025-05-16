using Microsoft.EntityFrameworkCore;
using Thrume.Common;
using Thrume.Database;
using Thrume.Domain.Entity;
using Thrume.Domain.EntityIds;

namespace Thrume.Services;

public sealed class PostService
{
    private readonly AppDbContext _dbContext;

    public PostService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task LikePostAsync(AccountId accountId, PostId postId)
    {
        var account = await _dbContext.AccountDbSet.FindAsync(accountId);
        var post = await _dbContext.PostDbSet
                                    .Include(p => p.LikedBy)
                                    .FirstOrDefaultAsync();

        if (account == null || post == null)
        {
            return;
        }

        if (post.LikedBy.Any(a => a.Id == accountId))
        {
            return;
        }

        post.LikedBy.Add(account);
        await _dbContext.SaveChangesAsync();

        return;
    }

    public async Task UnlikePostAsync(AccountId accountId, PostId postId)
    {
        var account = await _dbContext.AccountDbSet.FindAsync(accountId);
        var post = await _dbContext.PostDbSet
            .Include(p => p.LikedBy)
            .FirstOrDefaultAsync(p => p.Id == postId);

        if (account == null || post == null)
        {
            return;
        }

        var likedAccount = post.LikedBy.FirstOrDefault(a => a.Id == accountId);
        if (likedAccount == null)
        {
            return;
        }

        post.LikedBy.Remove(likedAccount);
        await _dbContext.SaveChangesAsync();
        
    }

    public async Task<Post?> GetPostByIdAsync(PostId postId, AccountId? currentAccountId = null) =>
        await _dbContext.PostDbSet
            .Include(p => p.Author)
            .Include(p => p.LikedBy)
            .FirstOrDefaultAsync(p => p.Id == postId);
}