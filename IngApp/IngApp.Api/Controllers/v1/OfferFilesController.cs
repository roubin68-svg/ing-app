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
}
