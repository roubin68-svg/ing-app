using IngApp.Application.Features.Financial.DTO;

namespace IngApp.Application.Common.Interfaces.Financial;

/// <summary>
/// Service برای مدیریت پورسانت بازاریاب‌ها
/// </summary>
public interface ICommissionService
{
    /// <summary>
    /// محاسبه و پرداخت پورسانت برای Unlock Contact
    /// </summary>
    Task<CommissionResultDto?> ProcessUnlockContactCommissionAsync(
        Guid buyerUserId,
        Guid unlockTransactionId,
        long unlockAmountRial);

    /// <summary>
    /// محاسبه و پرداخت پورسانت برای خرید Subscription
    /// </summary>
    Task<CommissionResultDto?> ProcessSubscriptionCommissionAsync(
        Guid buyerUserId,
        Guid subscriptionId,
        long subscriptionAmountRial);

    /// <summary>
    /// دریافت لیست پورسانت‌های یک بازاریاب
    /// </summary>
    Task<List<CommissionTransactionDto>> GetVisitorCommissionsAsync(Guid visitorUserId, int page = 1, int pageSize = 20);

    /// <summary>
    /// دریافت مجموع پورسانت‌های یک بازاریاب
    /// </summary>
    Task<long> GetTotalCommissionAmountAsync(Guid visitorUserId);

    /// <summary>
    /// دریافت لیست پورسانت‌های یک بازاریاب (برای Admin)
    /// </summary>
    Task<List<CommissionTransactionDto>> GetVisitorCommissionsForAdminAsync(Guid visitorUserId, int page = 1, int pageSize = 20);

    /// <summary>
    /// دریافت مجموع پورسانت‌های یک بازاریاب (برای Admin)
    /// </summary>
    Task<long> GetTotalCommissionAmountForAdminAsync(Guid visitorUserId);
}










