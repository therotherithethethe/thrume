using Amazon.S3;
using Amazon.S3.Model;
using Thrume.Common;

namespace Thrume.Infrastructure;

public static class AmazonS3Extensions
{
    public static async Task<Result<PutObjectResponse>> PutResObjectAsync(this IAmazonS3 amazonS3, PutObjectRequest req)
    {
        try
        {
            return await amazonS3.PutObjectAsync(req);
        }
        catch (Exception e)
        {
            return Result<PutObjectResponse>.Failure(e.ToProblemDetails(-1));
        }
    }
}