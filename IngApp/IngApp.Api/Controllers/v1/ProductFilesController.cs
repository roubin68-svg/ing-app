using System.Linq;
using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Interfaces.Products;
using IngApp.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;

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
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResult.Fail("فایلی ارسال نشده است."));

            // Validate file size (max 20MB)
            if (file.Length > 20_000_000)
                return BadRequest(ApiResult.Fail("حجم فایل نباید بیشتر از 20 مگابایت باشد."));

            // Validate file type (only images)
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
            var fileExtension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(fileExtension) || !allowedExtensions.Contains(fileExtension))
                return BadRequest(ApiResult.Fail("فقط فایل‌های تصویری مجاز هستند (jpg, jpeg, png, gif, webp, bmp)."));

            // Validate that product exists
            var product = await _productService.GetByIdAsync(productId);
            if (product == null)
                return NotFound(ApiResult.Fail("محصول یافت نشد."));

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
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResult.Fail($"خطا در فایل ارسالی: {ex.Message}"));
        }
        catch (DirectoryNotFoundException ex)
        {
            return StatusCode(500, ApiResult.Fail($"خطا در ایجاد پوشه ذخیره‌سازی: {ex.Message}"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(500, ApiResult.Fail($"خطا در دسترسی به فایل: {ex.Message}"));
        }
        catch (IOException ex)
        {
            return StatusCode(500, ApiResult.Fail($"خطا در ذخیره فایل: {ex.Message}"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult.Fail($"خطای غیرمنتظره در آپلود تصویر: {ex.Message}"));
        }
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

