using IngApp.Domain.Entities.Users;

namespace IngApp.Domain.Entities.Financial;

/// <summary>
/// اشتراک کاربر
/// </summary>
public class UserSubscription
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public int PlanId { get; set; }
    public Plan Plan { get; set; } = null!;
    
    public int StatusId { get; set; }
    public SubscriptionStatus Status { get; set; } = null!;
    
    /// <summary>
    /// تاریخ شروع اشتراک
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// تاریخ پایان اشتراک
    /// </summary>
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// شناسه تراکنش پرداخت (WalletTransaction)
    /// </summary>
    public Guid? PaymentTransactionId { get; set; }
    
    /// <summary>
    /// تاریخ خرید/فعال‌سازی
    /// </summary>
    public DateTime PurchasedAt { get; set; }
    
    /// <summary>
    /// تاریخ لغو (اگر لغو شده باشد)
    /// </summary>
    public DateTime? CancelledAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}











