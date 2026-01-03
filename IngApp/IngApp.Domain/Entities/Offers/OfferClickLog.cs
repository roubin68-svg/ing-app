using IngApp.Domain.Enums;

namespace IngApp.Domain.Entities.Offers;

/// <summary>
/// لاگ کلیک‌های روی آگهی‌ها
/// </summary>
public class OfferClickLog
{
    public int Id { get; set; }

    /// <summary>
    /// آگهی که روی آن کلیک شده
    /// </summary>
    public int OfferId { get; set; }
    public Offer Offer { get; set; } = null!;

    /// <summary>
    /// کاربری که کلیک کرده (Buyer) - می‌تواند null باشد برای بازدیدهای بدون لاگین
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// نوع کلیک: View (بازدید آگهی) یا ContactClick (کلیک روی اطلاعات تماس)
    /// </summary>
    public OfferClickType ClickType { get; set; }

    /// <summary>
    /// IP آدرس کاربر (اختیاری)
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User Agent (اختیاری)
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// زمان کلیک
    /// </summary>
    public DateTime ClickedAt { get; set; } = DateTime.UtcNow;
}

