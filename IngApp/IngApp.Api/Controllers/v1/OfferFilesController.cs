using System.Linq;
using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Interfaces.Offers;
using IngApp.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IngApp.Api.Controllers.v1;

[ApiController]
[Route("api/v1/offers/my/upload-file")]
[Authorize]
public class OfferFilesController : ControllerBase
{
    private readonly IOfferFileStorageService _fileStorage;
    private readonly IOfferService _offerService;

    public OfferFilesController(IOfferFileStorageService fileStorage, IOfferService offerService)
    {
        _fileStorage = fileStorage;
        _offerService = offerService;
    }


    private Guid GetCurrentUserId()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "uid");
        if (claim == null)
            throw new Exception("User id claim (uid) not found.");

        return Guid.Parse(claim.Value);
    }

    [HttpPost]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> UploadMyOfferFile(
        [FromForm] int offerId,
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            throw new ValidationException(new() { "فایلی ارسال نشده است." });

        var userId = GetCurrentUserId();

        await _offerService.EnsureEditableDraftAsync(userId, offerId);
        await using var stream = file.OpenReadStream();
        var relativePath = await _fileStorage.SaveAsync(
            userId,
            offerId,
            file.FileName,
            stream,
            cancellationToken);

        var response = new
        {
            FilePath = relativePath,
            OriginalFileName = file.FileName,
            Size = file.Length
        };

        return Ok(ApiResult.Ok(response));
    }

    /// <summary>
    /// دانلود فایل آگهی
    /// </summary>
    [HttpGet("file")]
    public async Task<IActionResult> DownloadFile(
        [FromQuery] int offerId,
        [FromQuery] string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return BadRequest();

        var userId = GetCurrentUserId();

        // بررسی دسترسی کاربر به آگهی
        var detail = await _offerService.GetDetailAsync(userId, offerId);

        // بررسی اینکه فایل متعلق به این آگهی است
        var doc = detail.Documents.FirstOrDefault(d => d.FilePath == filePath);
        if (doc == null)
            return NotFound();

        if (!_fileStorage.TryGetFileInfo(filePath, out var fullPath, out var contentType))
            return NotFound();

        var stream = System.IO.File.OpenRead(fullPath);
        var fileName = System.IO.Path.GetFileName(fullPath);
        var originalFileName = doc.Value ?? fileName;

        return File(stream, contentType, originalFileName);
    }

    /// <summary>
    /// حذف فایل آگهی (Soft Delete)
    /// </summary>
    [HttpDelete("file")]
    public async Task<IActionResult> DeleteFile(
        [FromQuery] int offerId,
        [FromQuery] string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return BadRequest(ApiResult.Fail("مسیر فایل الزامی است."));

        var userId = GetCurrentUserId();

        try
        {
            await _offerService.DeleteDocumentFileAsync(userId, offerId, filePath);
            return Ok(ApiResult.Ok("فایل با موفقیت حذف شد."));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResult.Fail(ex.Message));
        }
        catch (ValidationException ex)
        {
            return BadRequest(ApiResult.Fail(ex.Message));
        }
    }
}
