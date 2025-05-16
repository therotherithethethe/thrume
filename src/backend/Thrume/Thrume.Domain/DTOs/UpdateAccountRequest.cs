using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Thrume.Domain.Entity;
using Thrume.Domain.EntityIds;

namespace Thrume.Domain.DTOs;

public record struct UpdateAccountRequest(
    IFormFile? Picture = null,
    CreatePostRequest? PostReq = null);