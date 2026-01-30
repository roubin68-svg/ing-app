using IngApp.Application.Common.Interfaces.Financial;
using IngApp.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IngApp.Api.Controllers.v1;

[ApiController]
[Route("api/v1/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    private Guid? GetCurrentUserIdIfExists()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "uid");
        if (claim == null) return null;
        return Guid.TryParse(claim.Value, out var userId) ? userId : null;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "uid");
        if (claim == null)
            throw new UnauthorizedAccessException("کاربر احراز هویت نشده است.");
        return Guid.Parse(claim.Value);
    }

    // GET: دریافت لیست درگاه‌های پرداخت فعال
    [HttpGet("gateways")]
    public async Task<IActionResult> GetActiveGateways()
    {
        try
        {
            var gateways = await _paymentService.GetActiveGatewaysAsync();
            return Ok(ApiResult.Ok(gateways));
        }
        catch (Exception ex)
        {
            // Log error for debugging
            return StatusCode(500, ApiResult.Fail($"خطا در دریافت درگاه‌های پرداخت: {ex.Message}"));
        }
    }

    // POST: ایجاد درخواست TopUp (شارژ کیف پول)
    [HttpPost("topup")]
    [Authorize]
    public async Task<IActionResult> CreateTopUpRequest([FromBody] CreateTopUpRequest request)
    {
        var userId = GetCurrentUserId();
        
        // تولید IdempotencyKey
        var idempotencyKey = $"topup-{userId}-{request.AmountRial}-{DateTime.Now:yyyyMMddHHmmss}";
        
        var result = await _paymentService.CreateTopUpRequestAsync(
            userId,
            request.AmountRial,
            request.GatewayId,
            idempotencyKey);
        
        return Ok(ApiResult.Ok(result));
    }

    // POST: تایید پرداخت (Callback از درگاه)
    [HttpPost("verify/{paymentId:guid}")]
    public async Task<IActionResult> VerifyPayment(
        Guid paymentId,
        [FromBody] VerifyPaymentRequest? request = null)
    {
        var result = await _paymentService.VerifyPaymentAsync(
            paymentId,
            request?.GatewayTransactionId,
            request?.GatewayResponseJson);
        
        if (!result.Success)
        {
            return BadRequest(ApiResult.Fail(result.ErrorMessage ?? "خطا در تایید پرداخت"));
        }
        
        return Ok(ApiResult.Ok(result));
    }

    // GET: دریافت وضعیت پرداخت
    [HttpGet("{paymentId:guid}/status")]
    public async Task<IActionResult> GetPaymentStatus(Guid paymentId)
    {
        var status = await _paymentService.GetPaymentStatusAsync(paymentId);
        
        if (status == null)
            return NotFound(ApiResult.Fail("پرداخت یافت نشد."));
        
        return Ok(ApiResult.Ok(status));
    }
}

public class CreateTopUpRequest
{
    public long AmountRial { get; set; }
    public int GatewayId { get; set; }
}

public class VerifyPaymentRequest
{
    public string? GatewayTransactionId { get; set; }
    public string? GatewayResponseJson { get; set; }
}

