using IngApp.Application.Common.Interfaces.Offers;
using IngApp.Application.Common.Interfaces.Suppliers;
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Offers.Queries;
using IngApp.Domain.Enums;
using IngApp.Infrastructure.Services.Offers;
using Microsoft.AspNetCore.Mvc;

namespace IngApp.Api.Controllers.v1;

[ApiController]
[Route("api/v1/offers")]
public class OffersController : ControllerBase
{
    private readonly IOfferService _service;
    private readonly IOfferFileStorageService _fileStorage;
    private readonly IOfferClickService _clickService;
    private readonly ISupplierProfileService _supplierService;

    public OffersController(
        IOfferService service, 
        IOfferFileStorageService fileStorage, 
        IOfferClickService clickService,
        ISupplierProfileService supplierService)
    {
        _service = service;
        _fileStorage = fileStorage;
        _clickService = clickService;
        _supplierService = supplierService;
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
        
        // Log view click
        var userId = GetCurrentUserIdIfExists();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _clickService.LogClickAsync(offerId, OfferClickType.View, userId, ipAddress, userAgent);
        
        return Ok(ApiResult.Ok(result));
    }

    private Guid? GetCurrentUserIdIfExists()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "uid");
        if (claim == null) return null;
        return Guid.TryParse(claim.Value, out var userId) ? userId : null;
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
            return BadRequest(ApiResult.Fail("مسیر فایل الزامی است."));

        // بررسی اینکه آگهی Published باشد و فایل متعلق به آن باشد
        var detail = await _service.GetPublicDetailAsync(offerId);
        var doc = detail.Documents.FirstOrDefault(d => d.FilePath == filePath);
        
        if (doc == null)
            return NotFound(ApiResult.Fail("فایل موردنظر یافت نشد."));

        if (!_fileStorage.TryGetFileInfo(filePath, out var fullPath, out var contentType))
            return NotFound(ApiResult.Fail("فایل در سیستم یافت نشد."));

        var stream = System.IO.File.OpenRead(fullPath);
        var fileName = System.IO.Path.GetFileName(fullPath);
        var originalFileName = doc.Value ?? fileName;

        return File(stream, contentType, originalFileName);
    }

    // ---------------------------------------
    // Log Contact Click
    // ---------------------------------------
    [HttpPost("{offerId:int}/contact-click")]
    public async Task<IActionResult> LogContactClick(int offerId)
    {
        // بررسی اینکه آگهی Published باشد
        var detail = await _service.GetPublicDetailAsync(offerId);
        
        var userId = GetCurrentUserIdIfExists();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _clickService.LogClickAsync(offerId, OfferClickType.ContactClick, userId, ipAddress, userAgent);
        
        return Ok(ApiResult.Ok());
    }

    // ---------------------------------------
    // Check if user has viewed contact info
    // ---------------------------------------
    [HttpGet("{offerId:int}/has-viewed-contact")]
    public async Task<IActionResult> HasViewedContact(int offerId)
    {
        // بررسی اینکه آگهی Published باشد
        await _service.GetPublicDetailAsync(offerId);
        
        var userId = GetCurrentUserIdIfExists();
        var hasViewed = await _clickService.HasUserViewedContactAsync(offerId, userId);
        
        return Ok(ApiResult.Ok(new { hasViewed }));
    }

    // ---------------------------------------
    // Get Supplier Contact Info
    // ---------------------------------------
    [HttpGet("{offerId:int}/supplier-contact")]
    public async Task<IActionResult> GetSupplierContact(int offerId)
    {
        // بررسی اینکه آگهی Published باشد
        var detail = await _service.GetPublicDetailAsync(offerId);
        
        // دریافت اطلاعات supplier
        var supplier = await _supplierService.GetByUserIdAsync(detail.Header.SupplierUserId);
        if (supplier == null)
            return NotFound(ApiResult.Fail("تأمین‌کننده یافت نشد"));

        var contactInfo = new
        {
            BusinessName = supplier.BusinessName,
            SupplierTypeName = supplier.SupplierTypeName,
            ContactPhone = supplier.ContactPhone, // شماره تماس دفتر (اختیاری)
            Mobile = supplier.UserPhoneNumber, // موبایل از جدول Users (اعتبارسنجی شده)
            Address = supplier.Address,
            Province = supplier.Province,
            City = supplier.City
        };
        
        return Ok(ApiResult.Ok(contactInfo));
    }
}
