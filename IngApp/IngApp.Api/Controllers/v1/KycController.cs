using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Interfaces.Kyc;
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Kyc.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IngApp.Api.Controllers.v1
{
    [ApiController]
    [Route("api/v1/kyc")]
    [Authorize]
    public class KycController : ControllerBase
    {
        private readonly IKycService _kycService;
        private readonly IKycFileStorageService _fileStorage;

        public KycController(
            IKycService kycService,
            IKycFileStorageService fileStorage)
        {
            _kycService = kycService;
            _fileStorage = fileStorage;
        }

        // کمک‌کننده برای گرفتن UserId لاگین‌شده
        private Guid GetCurrentUserId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "uid");
            if (claim == null)
                throw new Exception("User id claim (uid) not found.");

            return Guid.Parse(claim.Value);
        }

        // =========================================================
        // ✅ برای خود کاربر (Supplier/Buyer)
        // =========================================================

        /// <summary>
        /// دریافت الزامات KYC برای کاربر فعلی (فرم داینامیک)
        /// </summary>
        [HttpGet("my/requirements")]
        public async Task<IActionResult> GetMyRequirements()
        {
            var userId = GetCurrentUserId();
            var data = await _kycService.GetRequirementsForUserAsync(userId);
            return Ok(ApiResult.Ok(data));
        }

        /// <summary>
        /// دریافت لیست مدارک KYC ارسال‌شده توسط کاربر فعلی
        /// </summary>
        [HttpGet("my/documents")]
        public async Task<IActionResult> GetMyDocuments()
        {
            var userId = GetCurrentUserId();
            var data = await _kycService.GetUserDocumentsAsync(userId);
            return Ok(ApiResult.Ok(data));
        }

        /// <summary>
        /// آپلود فایل KYC برای کاربر فعلی.
        /// خروجی: FilePath نسبی برای استفاده در SubmitKycDocumentItemDto
        /// </summary>
        [HttpPost("my/upload-file")]
        [RequestSizeLimit(20_000_000)] // مثلا 20MB - بعداً می‌تونی تنظیمش کنی
        public async Task<IActionResult> UploadMyDocumentFile([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ValidationException(new() { "فایلی ارسال نشده است." });

            var userId = GetCurrentUserId();

            await using var stream = file.OpenReadStream();
            var relativePath = await _fileStorage.SaveAsync(userId, file.FileName, stream);

            var response = new
            {
                FilePath = relativePath,
                OriginalFileName = file.FileName,
                Size = file.Length
            };

            return Ok(ApiResult.Ok(response));
        }

        /// <summary>
        /// ارسال/به‌روزرسانی مدارک KYC توسط کاربر فعلی
        /// (مسیر فایل‌ها قبلاً از طریق Upload گرفته شده است)
        /// </summary>
        [HttpPost("my/submit")]
        public async Task<IActionResult> SubmitMyDocuments([FromBody] List<SubmitKycDocumentItemDto> items)
        {
            var userId = GetCurrentUserId();
            await _kycService.SubmitDocumentsAsync(userId, items);
            return Ok(ApiResult.Ok());
        }

        // =========================================================
        // ✅ برای Admin
        // =========================================================

        /// <summary>
        /// لیست مدارک KYC (همه کاربران) با Paging/Filtering/Sorting
        /// </summary>
        [HttpGet("documents")]
        public async Task<IActionResult> GetDocuments([FromQuery] KycListQueryDto filter)
        {
            var result = await _kycService.GetPagedAsync(filter);
            return Ok(ApiResult.Ok(result));
        }

        /// <summary>
        /// دریافت جزئیات یک مدرک KYC با Id
        /// </summary>
        [HttpGet("documents/{id:guid}")]
        public async Task<IActionResult> GetDocumentById(Guid id)
        {
            var doc = await _kycService.GetDocumentByIdAsync(id);
            return Ok(ApiResult.Ok(doc));
        }

        /// <summary>
        /// بررسی/تأیید/رد یک مدرک KYC توسط Admin
        /// </summary>
        [HttpPut("documents/{id:guid}/review")]
        public async Task<IActionResult> ReviewDocument(Guid id, [FromBody] ReviewKycDocumentRequest request)
        {
            await _kycService.ReviewDocumentAsync(id, request);
            return Ok(ApiResult.Ok());
        }

        /// <summary>
        /// دانلود فایل مدرک KYC
        /// </summary>
        [HttpGet("documents/{id:guid}/file")]
        public async Task<IActionResult> DownloadDocumentFile(Guid id)
        {
            var doc = await _kycService.GetDocumentByIdAsync(id);

            if (doc == null || string.IsNullOrWhiteSpace(doc.FilePath))
                return NotFound();

            if (!_fileStorage.TryGetFileInfo(doc.FilePath, out var fullPath, out var contentType))
                return NotFound();

            var stream = System.IO.File.OpenRead(fullPath);
            var fileName = System.IO.Path.GetFileName(fullPath);

            return File(stream, contentType, fileName);
        }
    }
}
