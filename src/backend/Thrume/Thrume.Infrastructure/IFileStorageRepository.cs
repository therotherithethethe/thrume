using Thrume.Common;

namespace Thrume.Infrastructure;

public interface IFileStorageRepository
{
    Task<Result<string>> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task<bool> DeleteFileAsync(string fileName);
}