using IngApp.Domain.Entities.Users;

namespace IngApp.Domain.Entities.Financial;

/// <summary>
/// تراکنش پورسانت بازاریاب
/// </summary>
public class CommissionTransaction
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// شناسه بازاریاب (Visitor)
    /// </summary>
    public Guid VisitorUserId { get; set; }
    public User VisitorUser { get; set; } = null!;
    
    /// <summary>
    /// شناسه خریدار (Buyer) که معرفی شده
    /// </summary>
    public Guid BuyerUserId { get; set; }
    public User BuyerUser { get; set; } = null!;
    
    /// <summary>
    /// نوع پورسانت (UnlockContactCommission, SubscriptionCommission)
    /// </summary>
    public string CommissionType { get; set; } = null!;
    
    /// <summary>
    /// مبلغ اصلی تراکنش (به ریال)
    /// </summary>
    public long OriginalAmountRial { get; set; }
    
    /// <summary>
    /// مبلغ پورسانت (به ریال)
    /// </summary>
    public long CommissionAmountRial { get; set; }
    
    /// <summary>
    /// درصد پورسانت اعمال شده
    /// </summary>
    public decimal CommissionPercentage { get; set; }
    
    /// <summary>
    /// شناسه CommissionRule استفاده شده (اگر از قانون پیش‌فرض استفاده شده باشد)
    /// </summary>
    public int? CommissionRuleId { get; set; }
    public CommissionRule? CommissionRule { get; set; }
    
    /// <summary>
    /// شناسه VisitorCommissionRule استفاده شده (اگر از قانون اختصاصی بازاریاب استفاده شده باشد)
    /// </summary>
    public int? VisitorCommissionRuleId { get; set; }
    public VisitorCommissionRule? VisitorCommissionRule { get; set; }
    
    /// <summary>
    /// شناسه تراکنش Wallet (پورسانت پرداخت شده)
    /// </summary>
    public Guid? WalletTransactionId { get; set; }
    
    /// <summary>
    /// شناسه مرجع اصلی (مثلاً OfferContactUnlockId یا UserSubscriptionId)
    /// </summary>
    public Guid? ReferenceId { get; set; }
    
    /// <summary>
    /// نوع مرجع (OfferContactUnlock, UserSubscription)
    /// </summary>
    public string? ReferenceType { get; set; }
    
    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}










