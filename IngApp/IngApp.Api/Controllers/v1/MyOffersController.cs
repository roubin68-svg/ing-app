using IngApp.Application.Common.Interfaces.Offers;
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Offers.Queries;
using IngApp.Application.Features.Offers.Requests;
using IngApp.Infrastructure.Services.Offers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IngApp.Api.Controllers.v1;

[ApiController]
[Authorize]
[Route("api/v1/offers/my")]
public class MyOffersController : ControllerBase
{
    private readonly IOfferService _service;

    public MyOffersController(IOfferService service)
    {
        _service = service;
    }

    private Guid GetUserId()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "uid");
        return Guid.Parse(claim!.Value);
    }

    // --------------------------------------------------
    // Create Draft
    // --------------------------------------------------
    [HttpPost]
    public async Task<IActionResult> CreateDraft(
        [FromBody] CreateDraftOfferRequest request)
    {
        var userId = GetUserId();
        var offerId = await _service.CreateDraftAsync(userId, request);
        return Ok(ApiResult.Ok(new { offerId }));
    }

    // --------------------------------------------------
    // My Offers List
    // --------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> GetMyOffers([FromQuery] MyOffersQuery query)
    {
        var userId = GetUserId();
        var result = await _service.GetMyOffersAsync(userId, query);
        return Ok(ApiResult.Ok(result));
    }

    // --------------------------------------------------
    // My Offer Detail
    // --------------------------------------------------
    [HttpGet("{offerId:int}")]
    public async Task<IActionResult> GetDetail(int offerId)
    {
        var userId = GetUserId();
        var result = await _service.GetDetailAsync(userId, offerId);
        return Ok(ApiResult.Ok(result));
    }

    [HttpPut("{offerId:int}/product")]
    public async Task<IActionResult> ChangeProduct(
    int offerId,
    [FromBody] ChangeOfferProductRequest request)
    {
        var userId = GetUserId();
        await _service.ChangeProductAsync(userId, offerId, request);
        return Ok(ApiResult.Ok());
    }


    // --------------------------------------------------
    // Update Header (Draft only)
    // --------------------------------------------------
    [HttpPut("{offerId:int}/header")]
    public async Task<IActionResult> UpdateHeader(
        int offerId,
        [FromBody] UpdateOfferHeaderRequest request)
    {
        var userId = GetUserId();
        await _service.UpdateHeaderAsync(userId, offerId, request);
        return Ok(ApiResult.Ok());
    }

    // --------------------------------------------------
    // Save Documents (Attributes)
    // --------------------------------------------------
    [HttpPut("{offerId:int}/documents")]
    public async Task<IActionResult> SaveDocuments(
        int offerId,
        [FromBody] SaveOfferDocumentsRequest request)
    {
        var userId = GetUserId();
        await _service.SaveDocumentsAsync(userId, offerId, request);
        return Ok(ApiResult.Ok());
    }

    // --------------------------------------------------
    // Submit
    // --------------------------------------------------
    [HttpPost("{offerId:int}/submit")]
    public async Task<IActionResult> Submit(int offerId)
    {
        var userId = GetUserId();
        await _service.SubmitAsync(userId, offerId);
        return Ok(ApiResult.Ok());
    }

    // --------------------------------------------------
    // Cancel
    // --------------------------------------------------
    [HttpPost("{offerId:int}/cancel")]
    public async Task<IActionResult> Cancel(
        int offerId,
        [FromBody] string? reason)
    {
        var userId = GetUserId();
        await _service.CancelAsync(userId, offerId, reason);
        return Ok(ApiResult.Ok());
    }

    [HttpGet("available-products")]
    public async Task<IActionResult> GetAvailableProductsForOffer()
    {
        var userId = GetUserId();

        var result = await _service.GetAvailableProductsForOfferAsync(userId);

        return Ok(ApiResult.Ok(result));
    }
}
