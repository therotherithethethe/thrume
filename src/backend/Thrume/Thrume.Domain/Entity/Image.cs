using Thrume.Domain.EntityIds;

namespace Thrume.Domain.Entity;

public sealed class Image
{
    public ImageId Id { get; init; }
    public Post Post { get; init; }
}