using IngApp.Application.Common.Interfaces.Financial;
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Financial.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IngApp.Api.Controllers.v1;

[ApiController]
[Route("api/v1/user-subscriptions")]
[Authorize] // TODO: باید Permission مناسب اضافه شود
public class UserSubscriptionsController : ControllerBase
{
    private readonly IUserSubscriptionManagementService _subscriptionService;

    public UserSubscriptionsController(IUserSubscriptionManagementService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    // GET: دریافت لیست اشتراک‌ها با Pagination و فیلتر
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged([FromQuery] UserSubscriptionListQueryDto query)
    {
        var result = await _subscriptionService.GetPagedSubscriptionsAsync(query);
        return Ok(ApiResult.Ok(result));
    }
}










