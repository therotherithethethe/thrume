using Thrume.Common;

namespace Thrume.Infrastructure;

public static class ImageValidation
{
    private static readonly Dictionary<string, List<byte[]>> _fileSignature = new() //TODO
    {
        ["jpeg"] = [
            [0xFF, 0xD8],
        ],
        ["jpg"] = [
            [0xFF, 0xD8],
        ],
        ["png"] = [
            [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]
        ]
    };
    public static (bool, ProblemDetails?) IsImageValid(Stream fileStream, string fileName, string contentType)
    {
        if (string.IsNullOrEmpty(contentType) || !contentType.Contains("image"))
            return (false, DomainErrors.InvalidImageType);
        
        var key = fileName.Split('.')[^1];
        if(!_fileSignature.TryGetValue(key, out var signatures))
            return (false, DomainErrors.InvalidImageType);

        if (fileStream.Length >= 10 * 1024 * 1024) //10 MB
            return (false, DomainErrors.ImageToLarge);
        
        var binReader = new BinaryReader(fileStream);
        var headerBytes = binReader.ReadBytes(signatures.Max(m => m.Length));

        return (signatures.Any(signature => 
            headerBytes.Take(signature.Length).SequenceEqual(signature)), null);
    }
}