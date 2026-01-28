using IngApp.Application.Features.Financial.DTO;

namespace IngApp.Application.Common.Interfaces.Financial;

/// <summary>
/// Service برای مدیریت پرداخت‌ها و TopUp
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// ایجاد درخواست TopUp (شارژ کیف پول)
    /// </summary>
    Task<TopUpRequestResultDto> CreateTopUpRequestAsync(Guid userId, long amountRial, int gatewayId, string idempotencyKey);

    /// <summary>
    /// تایید پرداخت (Callback از درگاه پرداخت)
    /// </summary>
    Task<PaymentVerificationResultDto> VerifyPaymentAsync(Guid paymentId, string? gatewayTransactionId, string? gatewayResponseJson);

    /// <summary>
    /// دریافت وضعیت پرداخت
    /// </summary>
    Task<PaymentStatusDto?> GetPaymentStatusAsync(Guid paymentId);

    /// <summary>
    /// دریافت لیست درگاه‌های پرداخت فعال
    /// </summary>
    Task<List<PaymentGatewayDto>> GetActiveGatewaysAsync();
}











