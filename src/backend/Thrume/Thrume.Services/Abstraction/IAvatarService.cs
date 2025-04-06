namespace Thrume.Services.Abstraction;

public interface IAvatarService
{
    ValueTask<bool> IsFileValid(Stream fileStream, string fileName, string contentType); //TODO
}