using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Thrume.Domain.EntityIds;
using Thrume.Services;

namespace Thrume.Api.Endpoints;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CallController : ControllerBase
{
    private readonly ICallStateService _callStateService;
    private readonly ILogger<CallController> _logger;

    public CallController(ICallStateService callStateService, ILogger<CallController> logger)
    {
        _callStateService = callStateService;
        _logger = logger;
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetCallHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var history = await _callStateService.GetUserCallHistoryAsync(userId.Value);
            
            // Apply pagination
            var skip = (page - 1) * pageSize;
            var pagedHistory = history.Skip(skip).Take(pageSize).ToArray();

            var response = new
            {
                calls = pagedHistory.Select(call => new
                {
                    id = call.Id,
                    callerId = call.CallerId.ToString(),
                    calleeId = call.CalleeId.ToString(),
                    type = call.Type.ToString(),
                    status = call.Status.ToString(),
                    startedAt = call.StartedAt,
                    connectedAt = call.ConnectedAt,
                    endedAt = call.EndedAt,
                    duration = GetCallDuration(call),
                    rejectionReason = call.RejectionReason
                }),
                totalCount = history.Length,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)history.Length / pageSize)
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving call history for user");
            return StatusCode(500, "Failed to retrieve call history");
        }
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveCall()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var activeCall = await _callStateService.GetActiveCallAsync(userId.Value);
            
            if (activeCall == null)
            {
                return Ok(new { hasActiveCall = false });
            }

            var response = new
            {
                hasActiveCall = true,
                call = new
                {
                    id = activeCall.Id,
                    callerId = activeCall.CallerId.ToString(),
                    calleeId = activeCall.CalleeId.ToString(),
                    type = activeCall.Type.ToString(),
                    status = activeCall.Status.ToString(),
                    startedAt = activeCall.StartedAt,
                    connectedAt = activeCall.ConnectedAt
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active call for user");
            return StatusCode(500, "Failed to retrieve active call");
        }
    }

    [HttpGet("{callId}")]
    public async Task<IActionResult> GetCall(string callId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var call = await _callStateService.GetCallAsync(callId);
            
            if (call == null)
            {
                return NotFound("Call not found");
            }

            // Check if user is participant in the call
            if (call.CallerId != userId.Value && call.CalleeId != userId.Value)
            {
                return Forbid("Not authorized to view this call");
            }

            var response = new
            {
                id = call.Id,
                callerId = call.CallerId.ToString(),
                calleeId = call.CalleeId.ToString(),
                type = call.Type.ToString(),
                status = call.Status.ToString(),
                startedAt = call.StartedAt,
                connectedAt = call.ConnectedAt,
                endedAt = call.EndedAt,
                duration = GetCallDuration(call),
                rejectionReason = call.RejectionReason
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving call {CallId}", callId);
            return StatusCode(500, "Failed to retrieve call");
        }
    }

    [HttpGet("availability/{userId}")]
    public async Task<IActionResult> CheckUserAvailability(string userId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(userId, out var targetUserGuid))
            {
                return BadRequest("Invalid user ID");
            }

            var targetUserId = new AccountId(targetUserGuid);
            var canCall = await _callStateService.CanUserCallAsync(currentUserId.Value, targetUserId);
            var isAvailable = await _callStateService.IsUserAvailableForCallAsync(targetUserId);

            var response = new
            {
                userId,
                isAvailable,
                canCall
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user availability for {UserId}", userId);
            return StatusCode(500, "Failed to check user availability");
        }
    }

    private AccountId? GetCurrentUserId()
    {
        var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userId))
        {
            return new AccountId(userId);
        }
        return null;
    }

    private int? GetCallDuration(Services.Models.Call call)
    {
        if (call.ConnectedAt.HasValue && call.EndedAt.HasValue)
        {
            return (int)(call.EndedAt.Value - call.ConnectedAt.Value).TotalSeconds;
        }
        return null;
    }
}