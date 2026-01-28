using IngApp.Application.Common.Interfaces.Financial;
using IngApp.Application.Common.Interfaces.Suppliers;
using IngApp.Application.Common.Models;
using IngApp.Application.Common.Security;
using IngApp.Application.Features.Suppliers.DTO;
using IngApp.Infrastructure.Services.Suppliers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IngApp.Api.Controllers.v1
{
    [ApiController]
    [Route("api/v1/supplier-profiles")]
    [Authorize]
    public class SupplierProfilesController : ControllerBase
    {
        private readonly ISupplierProfileService _service;
        private readonly ISupplierOnboardingService _onboardingService;

        public SupplierProfilesController(
            ISupplierProfileService service,
            ISupplierOnboardingService onboardingService)
        {
            _service = service;
            _onboardingService = onboardingService;
        }

        // کمک‌کننده برای گرفتن UserId لاگین‌شده (همان الگوی KycController)
        private Guid GetCurrentUserId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "uid");
            if (claim == null)
                throw new Exception("User id claim (uid) not found.");

            return Guid.Parse(claim.Value);
        }

        // GET: لیست تأمین‌کننده‌ها (Paging / Filter / Sort)
        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] SupplierListQueryDto filter)
        {
            var data = await _service.GetPagedAsync(filter);
            return Ok(ApiResult.Ok(data));
        }

        // GET: api/v1/supplier-profiles/paged
        // این فقط برای سازگاری با فرانت (مثل Roles) اضافه شده
        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedAliased([FromQuery] SupplierListQueryDto filter)
        {
            var data = await _service.GetPagedAsync(filter);
            return Ok(ApiResult.Ok(data));
        }


        // GET: جزئیات یک تأمین‌کننده برای Admin
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _service.GetSupplierDetailAsync(id);
            if (item == null)
                return NotFound(ApiResult.Fail("تأمین‌کننده یافت نشد."));

            return Ok(ApiResult.Ok(item));
        }

        // PUT: تغییر وضعیت تأمین‌کننده (Approve / Reject)
        [HttpPut("{id:guid}/verification-status")]
        public async Task<IActionResult> UpdateVerificationStatus(
            Guid id,
            [FromBody] UpdateSupplierVerificationStatusRequest request)
        {
            var adminUserId = GetCurrentUserId().ToString();

            await _service.UpdateVerificationStatusAsync(
                id,
                request.Status,
                request.Note,
                adminUserId);

            return Ok(ApiResult.Ok());
        }

        // GET: تاریخچه وضعیت تأمین‌کننده
        [HttpGet("{id:guid}/verification-history")]
        public async Task<IActionResult> GetVerificationHistory(Guid id)
        {
            var history = await _service.GetVerificationHistoryAsync(id);
            return Ok(ApiResult.Ok(history));
        }

        // GET: لاگ فعالیت‌های تأمین‌کننده
        [HttpGet("{id:guid}/activity-logs")]
        public async Task<IActionResult> GetActivityLogs(Guid id)
        {
            var logs = await _service.GetActivityLogsAsync(id);
            return Ok(ApiResult.Ok(logs));
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMy()
        {
            var userId = GetCurrentUserId();

            var profile = await _service.GetMyAsync(userId);

            if (profile == null)
                return NotFound(ApiResult.Fail("پروفایل تأمین‌کننده یافت نشد."));

            return Ok(ApiResult.Ok(profile));
        }

        [HttpPut("my")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpsertSupplierProfileRequest request)
        {
            var userId = GetCurrentUserId(); // همونی که قبلاً استفاده می‌کنی

            await _service.UpsertForUserAsync(userId, request);

            return NoContent();
        }

        [HttpGet("pending-count")]
        public async Task<IActionResult> GetPendingCount()
        {
            var count = await _service.GetPendingCountAsync();
            return Ok(ApiResult.Ok(count));
        }

        // POST: ارسال پروفایل برای بررسی (با پرداخت Onboarding Fee)
        [HttpPost("my/submit")]
        public async Task<IActionResult> SubmitMyProfile()
        {
            var userId = GetCurrentUserId();

            // بررسی و پرداخت Onboarding Fee (اگر قبلاً پرداخت نشده باشد)
            var idempotencyKey = $"supplier-onboarding-{userId}";
            var onboardingResult = await _onboardingService.PayOnboardingFeeAsync(userId, idempotencyKey);

            if (!onboardingResult.HasPaid)
            {
                return BadRequest(ApiResult.Fail(
                    onboardingResult.ErrorMessage ?? "خطا در پرداخت هزینه ثبت‌نام. لطفاً موجودی کیف پول خود را بررسی کنید."));
            }

            // ارسال پروفایل برای بررسی
            await _service.SubmitForUserAsync(userId);

            return Ok(ApiResult.Ok(new
            {
                Message = "پروفایل با موفقیت ارسال شد.",
                OnboardingFee = onboardingResult.Charged ? new
                {
                    AmountRial = onboardingResult.ChargedAmountRial,
                    AmountToman = onboardingResult.ChargedAmountToman,
                    TransactionId = onboardingResult.TransactionId
                } : null
            }));
        }

        // GET: بررسی اینکه آیا Onboarding Fee پرداخت شده است
        [HttpGet("my/has-paid-onboarding")]
        public async Task<IActionResult> HasPaidOnboarding()
        {
            var userId = GetCurrentUserId();
            var hasPaid = await _onboardingService.HasPaidOnboardingFeeAsync(userId);
            return Ok(ApiResult.Ok(new { hasPaid }));
        }
    }
}
