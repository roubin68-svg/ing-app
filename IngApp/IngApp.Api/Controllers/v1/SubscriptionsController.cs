using IngApp.Application.Common.Interfaces.Financial;
using IngApp.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IngApp.Api.Controllers.v1;

[ApiController]
[Route("api/v1/subscriptions")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionsController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "uid");
        if (claim == null)
            throw new UnauthorizedAccessException("کاربر احراز هویت نشده است.");
        return Guid.Parse(claim.Value);
    }

    // GET: دریافت لیست پلن‌های فعال
    [HttpGet("plans")]
    public async Task<IActionResult> GetActivePlans()
    {
        var plans = await _subscriptionService.GetActivePlansAsync();
        return Ok(ApiResult.Ok(plans));
    }

    // GET: دریافت اشتراک فعال کاربر
    [HttpGet("my/active")]
    public async Task<IActionResult> GetMyActiveSubscription()
    {
        var userId = GetCurrentUserId();
        var subscription = await _subscriptionService.GetActiveSubscriptionAsync(userId);
        return Ok(ApiResult.Ok(subscription));
    }

    // GET: دریافت تاریخچه اشتراک‌های کاربر
    [HttpGet("my/history")]
    public async Task<IActionResult> GetMySubscriptionHistory()
    {
        var userId = GetCurrentUserId();
        var history = await _subscriptionService.GetUserSubscriptionHistoryAsync(userId);
        return Ok(ApiResult.Ok(history));
    }

    // POST: خرید اشتراک
    [HttpPost("purchase")]
    public async Task<IActionResult> PurchaseSubscription([FromBody] PurchaseSubscriptionRequest request)
    {
        var userId = GetCurrentUserId();
        
        // تولید IdempotencyKey
        var idempotencyKey = $"purchase-subscription-{userId}-{request.PlanId}-{DateTime.UtcNow:yyyyMMddHHmmss}";
        
        var result = await _subscriptionService.PurchaseSubscriptionAsync(userId, request.PlanId, idempotencyKey);
        
        if (!result.Success)
        {
            return BadRequest(ApiResult.Fail(result.ErrorMessage ?? "خطا در خرید اشتراک"));
        }
        
        return Ok(ApiResult.Ok(result));
    }
}

public class PurchaseSubscriptionRequest
{
    public int PlanId { get; set; }
}











