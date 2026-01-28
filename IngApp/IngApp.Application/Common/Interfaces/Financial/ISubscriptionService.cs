using IngApp.Application.Features.Financial.DTO;

namespace IngApp.Application.Common.Interfaces.Financial;

/// <summary>
/// Service برای مدیریت اشتراک‌ها
/// </summary>
public interface ISubscriptionService
{
    /// <summary>
    /// دریافت لیست پلن‌های فعال
    /// </summary>
    Task<List<PlanDto>> GetActivePlansAsync();

    /// <summary>
    /// دریافت اشتراک فعال کاربر
    /// </summary>
    Task<UserSubscriptionDto?> GetActiveSubscriptionAsync(Guid userId);

    /// <summary>
    /// بررسی اینکه آیا کاربر اشتراک فعال با UnlimitedContactViews دارد
    /// </summary>
    Task<bool> HasUnlimitedContactViewsAsync(Guid userId);

    /// <summary>
    /// خرید اشتراک
    /// </summary>
    Task<PurchaseSubscriptionResultDto> PurchaseSubscriptionAsync(Guid userId, int planId, string idempotencyKey);

    /// <summary>
    /// دریافت تاریخچه اشتراک‌های کاربر
    /// </summary>
    Task<List<UserSubscriptionDto>> GetUserSubscriptionHistoryAsync(Guid userId);
}










