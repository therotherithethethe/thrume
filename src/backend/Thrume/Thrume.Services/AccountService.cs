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
    public async Task UpdateAvatarAsync(AccountId accountId, IFormFile file)
    {
        var result = await _imageRepository.UploadFileAsync(file.OpenReadStream(), file.FileName, file.ContentType);
        if (result.IsSuccess) 
            await _dbContext.AccountDbSet
                .Where(acc => acc.Id == accountId)
                .ExecuteUpdateAsync(setters 
                    => setters.SetProperty(a => a.PictureUrl, result.Value));
    }

    public async Task CreatePostsAsync(AccountId id, CreatePostRequest request)
    {
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
}