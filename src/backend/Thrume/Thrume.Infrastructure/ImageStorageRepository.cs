using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Thrume.Common;
using Thrume.Configuration;
using static Thrume.Common.Result<string>;
using ImageUrl = string;

namespace Thrume.Infrastructure;

public sealed class ImageStorageRepository : IFileStorageRepository
{
    private readonly IAmazonS3 _s3Client;
    private readonly MinioConfiguration _configuration;
    private readonly ILogger<ImageStorageRepository> _logger;

    public ImageStorageRepository(IAmazonS3 s3Client, IOptions<MinioConfiguration> settings,
        ILogger<ImageStorageRepository> logger)
    {
        _s3Client = s3Client;
        _configuration = settings.Value;
        _logger = logger;
    }

    public async Task<Result<ImageUrl>> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        (var isImageValid, ProblemDetails? problemDetails) = ImageValidation.IsImageValid(fileStream, fileName, contentType);
        if (!isImageValid)
        {
            return Failure((ProblemDetails)problemDetails);
        }

        fileName = Guid.CreateVersion7().ToString();
        var putRequest = new PutObjectRequest
        {
            BucketName = _configuration.BucketName,
            Key = fileName,
            InputStream = fileStream,
            ContentType = contentType
        };

        var putObjectResponse = await _s3Client.PutResObjectAsync(putRequest);
        _logger.LogInformation("File '{FileName}' uploaded successfully to bucket '{BucketName}'.", fileName, _configuration.BucketName);
        return  putObjectResponse.IsSuccess 
            ? $"{_configuration.ServiceUrl}/{_configuration.BucketName}/{fileName}"
               : Failure(putObjectResponse.Errors);
    }

    public async Task<bool> DeleteFileAsync(string fileName)
    {
        throw new NotImplementedException();
    }
}