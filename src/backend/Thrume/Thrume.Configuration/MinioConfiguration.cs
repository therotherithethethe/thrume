namespace Thrume.Configuration;

public class MinioConfiguration
{
    public string ServiceUrl { get; init; } = null!;
    public string AccessKey { get; init; } = null!;
    public string SecretKey { get; init; } = null!;
    public string BucketName { get; init; } = null!;
    public bool UseSsl { get; init; }
}