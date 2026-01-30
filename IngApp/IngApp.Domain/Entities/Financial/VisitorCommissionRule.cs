namespace IngApp.Domain.Entities.Financial;

/// <summary>
/// قانون پورسانت اختصاصی برای هر Visitor
/// اگر برای یک Visitor تعریف نشده باشد، از CommissionRule پیش‌فرض استفاده می‌شود
/// </summary>
public class VisitorCommissionRule
{
    public int Id { get; set; }
    
    /// <summary>
    /// شناسه VisitorProfile
    /// </summary>
    public Guid VisitorProfileId { get; set; }
    public Users.VisitorProfile VisitorProfile { get; set; } = null!;
    
    /// <summary>
    /// کد قانون (UnlockContactCommission, SubscriptionCommission)
    /// </summary>
    public string CommissionRuleCode { get; set; } = null!;
    
    /// <summary>
    /// درصد پورسانت اختصاصی برای این Visitor (مثلاً 5 برای 5%)
    /// اگر null باشد، از CommissionRule پیش‌فرض استفاده می‌شود
    /// </summary>
    public decimal? CommissionPercentage { get; set; }
    
    /// <summary>
    /// فعال/غیرفعال
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// تاریخ شروع اعتبار
    /// </summary>
    public DateTime? EffectiveFrom { get; set; }
    
    /// <summary>
    /// تاریخ پایان اعتبار
    /// </summary>
    public DateTime? EffectiveTo { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}












