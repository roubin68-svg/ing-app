using IngApp.Domain.Enums;

namespace IngApp.Application.Common.Interfaces.Offers;

/// <summary>
/// سرویس مدیریت کلیک‌های روی آگهی‌ها
/// </summary>
public interface IOfferClickService
{
    /// <summary>
    /// ثبت کلیک روی آگهی (View یا ContactClick)
    /// </summary>
    Task LogClickAsync(int offerId, OfferClickType clickType, Guid? userId = null, string? ipAddress = null, string? userAgent = null);

    /// <summary>
    /// دریافت تعداد کلیک‌های یک آگهی بر اساس نوع
    /// </summary>
    Task<int> GetClickCountAsync(int offerId, OfferClickType clickType);

    /// <summary>
    /// دریافت آمار کلیک‌های یک آگهی
    /// </summary>
    Task<OfferClickStatsDto> GetClickStatsAsync(int offerId);
}

/// <summary>
/// آمار کلیک‌های یک آگهی
/// </summary>
public class OfferClickStatsDto
{
    public int OfferId { get; set; }
    public int ViewCount { get; set; }
    public int ContactClickCount { get; set; }
}

