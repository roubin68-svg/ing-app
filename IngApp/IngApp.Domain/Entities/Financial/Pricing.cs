namespace IngApp.Domain.Entities.Financial;

/// <summary>
/// تعرفه‌های مالی (Pricing)
/// نگهداری تعرفه‌های مختلف سیستم مثل UnlockContactFee، OnboardingFee و ...
/// </summary>
public class Pricing
{
    public int Id { get; set; }

    /// <summary>
    /// کد یکتا برای شناسایی تعرفه (UnlockContactFee, OnboardingFee, ...)
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// عنوان قابل نمایش در UI (فارسی)
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// مبلغ تعرفه به ریال
    /// </summary>
    public long AmountRial { get; set; }

    /// <summary>
    /// تاریخ شروع اعتبار تعرفه (برای تغییر تعرفه‌ها در آینده)
    /// </summary>
    public DateTime? EffectiveFrom { get; set; }

    /// <summary>
    /// تاریخ پایان اعتبار تعرفه (null = نامحدود)
    /// </summary>
    public DateTime? EffectiveTo { get; set; }

    /// <summary>
    /// آیا این تعرفه فعال است؟
    /// </summary>
    public bool IsActive { get; set; } = true;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}












