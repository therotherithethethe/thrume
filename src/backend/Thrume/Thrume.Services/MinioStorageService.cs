using System.Collections.ObjectModel;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Thrume.Configuration;
using Thrume.Domain;
using Thrume.Services.Abstraction;
using Thrume.Services.Exceptions;
using ImageUrl = string;

namespace Thrume.Services;

public class MinioStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly MinioConfiguration _configuration;
    private readonly ILogger<MinioStorageService> _logger;
    private readonly IAvatarService _avatarService;
    
    public MinioStorageService(IAmazonS3 s3Client, IOptions<MinioConfiguration> settings, ILogger<MinioStorageService> logger, IAvatarService avatarService)
    {
        _s3Client = s3Client;
        _configuration = settings.Value;
        _logger = logger;
        _avatarService = avatarService;
    }

    public async Task<ImageUrl> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        try
        {
            await _avatarService.IsFileValid(fileStream, fileName, contentType);
            var putRequest = new PutObjectRequest
            {
                BucketName = _configuration.BucketName,
                Key = fileName,
                InputStream = fileStream,
                ContentType = contentType,
            };

            await _s3Client.PutObjectAsync(putRequest);
            _logger.LogInformation("File '{FileName}' uploaded successfully to bucket '{BucketName}'.", fileName, _configuration.BucketName);
            
            return $"{_configuration.ServiceUrl}/{_configuration.BucketName}/{fileName}";
        }
        catch (AmazonS3Exception e)
        {
            _logger.LogTrace(e, "S3 Error uploading file {FileName}: {ErrorMessage}", fileName, e.Message);
            throw; 
        }
        catch(Exception e) when (e is FileToLargeException or InvalidFileTypeException)
        {
            _logger.LogTrace(e, "Invalid file {FileName}", fileName);
            throw;
        }
        catch (Exception e)
        {
            _logger.LogTrace(e, "Error uploading file {FileName}", fileName);
            throw;
        }
    }

    public async Task DeleteFileAsync(string fileName)
    {
        throw new NotImplementedException();
    }
}