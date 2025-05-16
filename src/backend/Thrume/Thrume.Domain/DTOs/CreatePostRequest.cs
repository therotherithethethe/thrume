using Microsoft.AspNetCore.Http;
using Thrume.Domain.EntityIds;

namespace Thrume.Domain.DTOs;

public sealed record CreatePostRequest(string Content, IFormFileCollection Images);
