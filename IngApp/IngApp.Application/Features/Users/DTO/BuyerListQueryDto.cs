namespace IngApp.Application.Features.Users.DTO;

/// <summary>
/// DTO برای فیلتر و صفحه‌بندی لیست Buyer ها
/// </summary>
public class BuyerListQueryDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; }

    // فیلترها
    public string? Search { get; set; } // جستجو در شماره موبایل، نام، نام کسب‌وکار
    public Guid? ReferredByVisitorId { get; set; } // فیلتر بر اساس بازاریاب
    public string? ReferralCode { get; set; } // فیلتر بر اساس کد معرف بازاریاب
}



