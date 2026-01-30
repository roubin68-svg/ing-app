namespace IngApp.Application.Features.Financial.DTO;

/// <summary>
/// خلاصه کاربر با اطلاعات اشتراک‌ها
/// </summary>
public class UserWithSubscriptionsSummaryDto
{
    public Guid UserId { get; set; }
    public string UserPhoneNumber { get; set; } = null!;
    public string? UserDisplayName { get; set; }
    
    /// <summary>
    /// تعداد کل اشتراک‌های خریداری شده
    /// </summary>
    public int TotalSubscriptionsCount { get; set; }
    
    /// <summary>
    /// تعداد اشتراک‌های فعال
    /// </summary>
    public int ActiveSubscriptionsCount { get; set; }
    
    /// <summary>
    /// تعداد اشتراک‌های تمام شده
    /// </summary>
    public int ExpiredSubscriptionsCount { get; set; }
    
    /// <summary>
    /// لیست جزئیات اشتراک‌های این کاربر
    /// </summary>
    public List<UserSubscriptionDetailDto> Subscriptions { get; set; } = new();
}



