using Microsoft.Extensions.Logging;
using Thrume.Services.Abstraction;
using Thrume.Services.Exceptions;

namespace Thrume.Services;

public class AvatarFileService : IAvatarService //TODO: wouldn't be better to use static class..?
{
    private readonly ILogger<AvatarFileService> _logger;

    public AvatarFileService(ILogger<AvatarFileService> logger)
    {
        _logger = logger;
    }

    private static readonly Dictionary<string, List<byte[]>> _fileSignature = new() //TODO
    {
        ["jpeg"] = [
            [0xFF, 0xD8, 0xFF, 0xE0],
            [0xFF, 0xD8, 0xFF, 0xE2],
            [0xFF, 0xD8, 0xFF, 0xE3]
        ],
        ["jpg"] = [
            [0xFF, 0xD8, 0xFF, 0xE0],
            [0xFF, 0xD8, 0xFF, 0xE2],
            [0xFF, 0xD8, 0xFF, 0xE3]
        ],
        ["png"] = [
            [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]
        ]
    };
    public async ValueTask<bool> IsFileValid(Stream fileStream, string fileName, string contentType)
    {
        if (string.IsNullOrEmpty(contentType) || !contentType.Contains("image"))
            throw new InvalidFileTypeException(contentType);
        
        var key = fileName.Split('.')[1];
        if(!_fileSignature.TryGetValue(key, out var signatures))
            throw new InvalidFileTypeException(key);
        
        if (fileStream.Length >= 2097152) 
            throw new FileToLargeException("File size is large than 2 MB.");
        
        var binReader = new BinaryReader(fileStream);
        var headerBytes = binReader.ReadBytes(signatures.Max(m => m.Length));

        return signatures.Any(signature => 
            headerBytes.Take(signature.Length).SequenceEqual(signature));
    }
}