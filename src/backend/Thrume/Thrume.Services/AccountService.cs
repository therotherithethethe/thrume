using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Thrume.Common;
using Thrume.Database;
using Thrume.Domain.DTOs;
using Thrume.Domain.Entity;
using Thrume.Domain.EntityIds;
using Thrume.Infrastructure;

namespace Thrume.Services;

public sealed class AccountService
{
    private readonly AppDbContext _dbContext;
    private readonly IFileStorageRepository _imageRepository;

    public AccountService(AppDbContext dbContext, IFileStorageRepository imageRepository)
    {
        _dbContext = dbContext;
        _imageRepository = imageRepository;
    }
    public async Task<bool> UpdateAvatarAsync(AccountId accountId, IFormFile file)
    {
        var result = await _imageRepository.UploadFileAsync(file.OpenReadStream(), file.FileName, file.ContentType);
        if (result.IsSuccess)
        {
            await _dbContext.AccountDbSet
                .Where(acc => acc.Id == accountId)
                .ExecuteUpdateAsync(setters
                    => setters.SetProperty(a => a.PictureUrl, result.Value));
            return true;
        }
        else return false;
    }

    public async Task CreatePostsAsync(AccountId id, CreatePostRequest request)
    {
        if (request is { Content: [], Images: [] }) return;
        if (request.Images.Count >= 10) return;
        List<string> imageLinks = new() { Capacity = request.Images.Count };
        foreach (var formFile in request.Images)
        {
            var result = await _imageRepository.UploadFileAsync(formFile.OpenReadStream(), formFile.FileName,
                formFile.ContentType);
            if (result.IsFault) return;
            
            imageLinks.Add(result.Value!);
        }

        Account? account = await _dbContext.AccountDbSet.FindAsync(id);
        if (account is null) return;
        Post post = new()
        {
            Author = account,
            Content = request.Content,
            Images = imageLinks.Select(link => new Image {Id = new ImageId(link)}).ToList()
        };
        account.Posts.Add(post);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> FollowAsync(AccountId followerId, AccountId targetAccountId)
    {
        if (followerId.Equals(targetAccountId))
        {
            return false;
        }
        
        var followerAccount = await _dbContext.Users.AnyAsync(u => u.Id == followerId);
        if (!followerAccount)
        {
            return false; 
        }

        var targetAccountExists = await _dbContext.Users.AnyAsync(u => u.Id == targetAccountId);
        if (!targetAccountExists)
        {
            return false;
        }
        
        bool isAlreadyFollowing = await _dbContext.Subscriptions
            .AnyAsync(s => s.FollowerId == followerId && s.FollowingId == targetAccountId);

        if (isAlreadyFollowing)
        {
            return false;
        }

        var subscription = new Subscription
        {
            FollowerId = followerId.Value,
            FollowingId = targetAccountId.Value,
            SubscribedAtUtc = DateTime.UtcNow
        };

        _dbContext.Subscriptions.Add(subscription);
        
            await _dbContext.SaveChangesAsync();
            return true;
    }
    
    public async Task<bool> UnfollowAsync(AccountId followerId, AccountId targetAccountId)
    {
        if (followerId.Equals(targetAccountId))
        {
            return false;
        }

        var subscription = await _dbContext.Subscriptions
            .FirstOrDefaultAsync(s => s.FollowerId == followerId && s.FollowingId == targetAccountId);

        if (subscription == null)
        {
            return false; 
        }

        _dbContext.Subscriptions.Remove(subscription);


            await _dbContext.SaveChangesAsync();
            return true;
    }
    
}