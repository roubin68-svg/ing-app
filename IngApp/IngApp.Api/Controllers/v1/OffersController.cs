using IngApp.Application.Common.Interfaces.Offers;
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Offers.Queries;
using IngApp.Infrastructure.Services.Offers;
using Microsoft.AspNetCore.Mvc;

namespace IngApp.Api.Controllers.v1;

[ApiController]
[Route("api/v1/offers")]
public class OffersController : ControllerBase
{
    private readonly IOfferService _service;

    public OffersController(IOfferService service)
    {
        _service = service;
    }

    // ---------------------------------------
    // Public Search
    // ---------------------------------------
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] PublicOfferSearchQuery query)
    {
        var result = await _service.SearchPublicAsync(query);
        return Ok(ApiResult.Ok(result));
    }

    // ---------------------------------------
    // Public Detail
    // ---------------------------------------
    [HttpGet("{offerId:int}")]
    public async Task<IActionResult> GetDetail(int offerId)
    {
        var result = await _service.GetPublicDetailAsync(offerId);
        return Ok(ApiResult.Ok(result));
    }



}
