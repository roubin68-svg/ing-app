using IngApp.Application.Features.Financial.DTO;

namespace IngApp.Application.Common.Interfaces.Financial;

/// <summary>
/// Service برای مدیریت Unlock Contact آگهی‌ها
/// </summary>
public interface IUnlockContactService
{
    /// <summary>
    /// بررسی اینکه آیا کاربر قبلاً این آگهی را Unlock کرده است
    /// </summary>
    Task<bool> IsUnlockedAsync(int offerId, Guid userId);

    /// <summary>
    /// Unlock کردن Contact یک آگهی
    /// منطق:
    /// - اگر قبلاً Unlock شده باشد → بدون هزینه
    /// - اگر اشتراک فعال داشته باشد → بدون هزینه
    /// - در غیر اینصورت → از Wallet کسر می‌شود
    /// </summary>
    Task<UnlockContactResultDto> UnlockContactAsync(int offerId, Guid userId, string idempotencyKey);
}

