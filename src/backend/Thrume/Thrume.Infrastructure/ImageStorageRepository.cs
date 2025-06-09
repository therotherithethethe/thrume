using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Supabase.Storage;
using Thrume.Common;
using Thrume.Configuration;
using static Thrume.Common.Result<string>;
using Client = Supabase.Client;
using ImageUrl = string;

namespace Thrume.Infrastructure;

public sealed class ImageStorageRepository : IFileStorageRepository
{
    // private readonly MinioConfiguration _configuration;
    private readonly ILogger<ImageStorageRepository> _logger;
    private readonly Supabase.Client _supabaseClient;

    public ImageStorageRepository(
        ILogger<ImageStorageRepository> logger, Client supabaseClient)
    {
        // _s3Client = s3Client;
        // _configuration = settings.Value;
        _logger = logger;
        _supabaseClient = supabaseClient;
    }

    // public async Task<Result<ImageUrl>> UploadFileAsync1(Stream fileStream, string fileName, string contentType)
    // {
    //     (var isImageValid, ProblemDetails? problemDetails) = ImageValidation.IsImageValid(fileStream, fileName, contentType);
    //     if (!isImageValid)
    //     {
    //         return Failure((ProblemDetails)problemDetails);
    //     }
    //
    //     fileName = Guid.CreateVersion7().ToString();
    //     var putRequest = new PutObjectRequest
    //     {
    //         BucketName = _configuration.BucketName,
    //         Key = "fileName",
    //         InputStream = fileStream,
    //         ContentType = contentType,
    //     };
    //     var fileTransferUtilityRequest = new TransferUtilityUploadRequest
    //     {
    //         BucketName = _configuration.BucketName,
    //         InputStream = fileStream,
    //         StorageClass = S3StorageClass.StandardInfrequentAccess,
    //         Key = "sourceFileKey",
    //         CannedACL = S3CannedACL.PublicRead
    //     };
    //     var putObjectResponse = await _s3Client.PutResObjectAsync(putRequest);
    //     _logger.LogInformation("File '{FileName}' uploaded successfully to bucket '{BucketName}'.", fileName, _configuration.BucketName);
    //     return  putObjectResponse.IsSuccess 
    //         ? $"{_configuration.ServiceUrl}/{_configuration.BucketName}/{fileName}"
    //            : Failure(putObjectResponse.Errors);
    // }

    public async Task<Result<ImageUrl>> UploadFileAsync(Stream fileStream, string originalFileName, string contentType)
    {
        // 1. Конвертуємо потік в масив байтів (це залишається без змін)
        byte[] fileBytes;
        await using (var memoryStream = new MemoryStream())
        {
            await fileStream.CopyToAsync(memoryStream);
            fileBytes = memoryStream.ToArray();
        }

        // 2. Генеруємо унікальне ім'я, яке гарантовано є ASCII-сумісним.
        // Ми також зберігаємо оригінальне розширення файлу.
        var fileExtension = Path.GetExtension(originalFileName);
        var supabasePath = $"{Guid.NewGuid()}{fileExtension}"; // Наприклад: "d9e8e60e-....jpg"

        // 3. Створюємо об'єкт FileOptions і ЯВНО вказуємо ContentType.
        // Це найважливіший крок!
        var fileOptions = new Supabase.Storage.FileOptions
        {
            CacheControl = "3600", // Стандартна практика для кешування
            Upsert = false,
            // Передаємо ContentType, отриманий з IFormFile.
            // Це усуває необхідність автоматичного визначення і пов'язані з ним проблеми.
            ContentType = contentType 
        };

        // 4. Завантажуємо файл
        await _supabaseClient.Storage
            .From("images")
            .Upload(fileBytes, supabasePath, fileOptions);

        // 5. Отримуємо публічний URL (це залишається без змін)
        var publicUrl = _supabaseClient.Storage
            .From("images")
            .GetPublicUrl(supabasePath);
        
        // Припускаючи, що у вас є клас Result та ImageUrl
        return new ImageUrl(publicUrl); 
    }
    public async Task<bool> DeleteFileAsync(string fileName)
    {
        throw new NotImplementedException();
    }
}