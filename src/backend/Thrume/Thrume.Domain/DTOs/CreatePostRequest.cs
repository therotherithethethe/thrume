namespace Thrume.Domain.DTOs;

public sealed record CreatePostRequest(string Content) 
{
    public Entity.Post ToPost() => new() { Content = Content };
}
