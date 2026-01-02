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
    private readonly IOfferFileStorageService _fileStorage;

    public OffersController(IOfferService service, IOfferFileStorageService fileStorage)
    {
        _service = service;
        _fileStorage = fileStorage;
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

    // ---------------------------------------
    // Public File Download
    // ---------------------------------------
    [HttpGet("{offerId:int}/files")]
    public async Task<IActionResult> DownloadFile(
        [FromQuery] int offerId,
        [FromQuery] string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return BadRequest(ApiResult.Error("مسیر فایل الزامی است."));

        // بررسی اینکه آگهی Published باشد و فایل متعلق به آن باشد
        var detail = await _service.GetPublicDetailAsync(offerId);
        var doc = detail.Documents.FirstOrDefault(d => d.FilePath == filePath);
        
        if (doc == null)
            return NotFound(ApiResult.Error("فایل موردنظر یافت نشد."));

        if (!_fileStorage.TryGetFileInfo(filePath, out var fullPath, out var contentType))
            return NotFound(ApiResult.Error("فایل در سیستم یافت نشد."));

        var stream = System.IO.File.OpenRead(fullPath);
        var fileName = System.IO.Path.GetFileName(fullPath);
        var originalFileName = doc.Value ?? fileName;

        return File(stream, contentType, originalFileName);
    }
}
