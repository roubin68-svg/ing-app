namespace IngApp.Application.Features.Financial.DTO;

/// <summary>
/// Query برای دریافت لیست کاربران با خلاصه اشتراک‌ها
/// </summary>
public class UsersWithSubscriptionsQueryDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    
    /// <summary>
    /// فیلتر بر اساس شماره موبایل کاربر
    /// </summary>
    public string? UserPhoneNumber { get; set; }
    
    /// <summary>
    /// فیلتر بر اساس نام کاربر
    /// </summary>
    public string? UserDisplayName { get; set; }
}



