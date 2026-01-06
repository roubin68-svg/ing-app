using IngApp.Application.Common.Interfaces.Offers;
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Offers.DTO;
using IngApp.Application.Features.Offers.Queries;
using IngApp.Application.Features.Offers.Requests;
using IngApp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace IngApp.Api.Controllers.v1;

[ApiController]
[Route("api/v1/admin/offers")]
[Authorize]
public class AdminOffersController : ControllerBase
{
    private readonly IOfferService _offerService;

    public AdminOffersController(IOfferService offerService)
    {
        _offerService = offerService;
    }

    private string GetCurrentUserId()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "uid");
        if (claim == null)
            throw new Exception("User id claim (uid) not found.");
        return claim.Value;
    }

    /// <summary>
    /// لیست همه آگهی‌ها برای ادمین (با فیلتر تامین‌کننده و وضعیت)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetOffers([FromQuery] AdminOffersQuery query)
    {
        var result = await _offerService.GetAdminOffersAsync(query);
        return Ok(ApiResult.Ok(result));
    }

    /// <summary>
    /// رد کردن آگهی منتشر شده
    /// </summary>
    [HttpPut("{offerId}/reject")]
    public async Task<IActionResult> RejectOffer(
        [FromRoute] int offerId,
        [FromBody] RejectOfferRequest request)
    {
        var adminUserId = GetCurrentUserId();
        await _offerService.RejectOfferAsync(offerId, request, adminUserId);
        return Ok(ApiResult.Ok(new { message = "آگهی با موفقیت رد شد." }));
    }

    /// <summary>
    /// دریافت جزئیات یک آگهی برای ادمین (همه وضعیت‌ها)
    /// </summary>
    [HttpGet("{offerId}")]
    public async Task<IActionResult> GetOfferDetail([FromRoute] int offerId)
    {
        var result = await _offerService.GetAdminOfferDetailAsync(offerId);
        return Ok(ApiResult.Ok(result));
    }

    /// <summary>
    /// دریافت تاریخچه تغییر وضعیت آگهی
    /// </summary>
    [HttpGet("{offerId}/status-history")]
    public async Task<IActionResult> GetStatusHistory([FromRoute] int offerId)
    {
        var history = await _offerService.GetOfferStatusHistoryAsync(offerId);
        return Ok(ApiResult.Ok(history));
    }
}

