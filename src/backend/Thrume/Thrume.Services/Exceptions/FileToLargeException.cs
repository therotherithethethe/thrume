namespace Thrume.Services.Exceptions;

public class FileToLargeException(string? message = null) : Exception
{
    public override string Message { get; } = message ?? string.Empty;
}