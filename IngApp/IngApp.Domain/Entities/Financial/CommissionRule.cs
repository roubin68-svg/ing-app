namespace IngApp.Domain.Entities.Financial;

/// <summary>
/// قانون پورسانت بازاریاب
/// </summary>
public class CommissionRule
{
    public int Id { get; set; }
    
    /// <summary>
    /// کد قانون (UnlockContactCommission, SubscriptionCommission)
    /// </summary>
    public string Code { get; set; } = null!;
    
    /// <summary>
    /// عنوان
    /// </summary>
    public string Title { get; set; } = null!;
    
    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// درصد پورسانت (مثلاً 10 برای 10%)
    /// </summary>
    public decimal CommissionPercentage { get; set; }
    
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
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}










