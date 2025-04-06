namespace Thrume.Services.Exceptions;

public class InvalidFileTypeException(string? message = null) : Exception
{
    public override string Message { get; } = message ?? string.Empty;
}