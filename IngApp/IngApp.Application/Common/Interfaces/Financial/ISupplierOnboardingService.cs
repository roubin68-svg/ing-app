using IngApp.Application.Features.Financial.DTO;

namespace IngApp.Application.Common.Interfaces.Financial;

/// <summary>
/// Service برای مدیریت Supplier Onboarding Fee
/// </summary>
public interface ISupplierOnboardingService
{
    /// <summary>
    /// بررسی اینکه آیا کاربر قبلاً Onboarding Fee پرداخت کرده است
    /// </summary>
    Task<bool> HasPaidOnboardingFeeAsync(Guid userId);

    /// <summary>
    /// پرداخت Onboarding Fee
    /// منطق:
    /// - اگر قبلاً پرداخت شده باشد → بدون هزینه
    /// - در غیر اینصورت → از Wallet کسر می‌شود
    /// </summary>
    Task<SupplierOnboardingResultDto> PayOnboardingFeeAsync(Guid userId, string idempotencyKey);
}











