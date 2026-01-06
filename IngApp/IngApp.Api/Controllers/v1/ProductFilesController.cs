using System.Linq;
using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Interfaces.Products;
using IngApp.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IngApp.Api.Controllers.v1;

[ApiController]
[Route("api/v1/products/upload-image")]
[Authorize]
public class ProductFilesController : ControllerBase
{
    private readonly IProductFileStorageService _fileStorage;
    private readonly IProductService _productService;

    public ProductFilesController(IProductFileStorageService fileStorage, IProductService productService)
    {
        _fileStorage = fileStorage;
        _productService = productService;
    }

    [HttpPost]
    [RequestSizeLimit(20_000_000)] // 20MB
    public async Task<IActionResult> UploadProductImage(
        [FromForm] int productId,
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            throw new ValidationException(new() { "فایلی ارسال نشده است." });

        // Validate that product exists
        var product = await _productService.GetByIdAsync(productId);
        if (product == null)
            throw new NotFoundException("محصول یافت نشد.");

        await using var stream = file.OpenReadStream();
        var relativePath = await _fileStorage.SaveAsync(
            productId,
            file.FileName,
            stream,
            cancellationToken);

        // Update product ImagePath
        await _productService.UpdateAsync(productId, new IngApp.Application.Features.Products.DTO.UpdateProductRequest
        {
            Name = product.Name,
            CategoryId = product.CategoryId,
            Unit = product.Unit ?? string.Empty,
            ImagePath = relativePath
        });

        var response = new
        {
            FilePath = relativePath,
            OriginalFileName = file.FileName,
            Size = file.Length
        };

        return Ok(ApiResult.Ok(response));
    }

    /// <summary>
    /// دانلود تصویر محصول
    /// </summary>
    [HttpGet("image")]
    public async Task<IActionResult> DownloadImage(
        [FromQuery] int productId,
        [FromQuery] string filePath,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return BadRequest(ApiResult.Fail("مسیر فایل مشخص نشده است."));

        // Validate that product exists
        var product = await _productService.GetByIdAsync(productId);
        if (product == null)
            return NotFound(ApiResult.Fail("محصول یافت نشد."));

        if (!_fileStorage.TryGetFileInfo(filePath, out var fullPath, out var contentType))
            return NotFound(ApiResult.Fail("فایل یافت نشد."));

        var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath, cancellationToken);
        return File(fileBytes, contentType, Path.GetFileName(fullPath));
    }
}

