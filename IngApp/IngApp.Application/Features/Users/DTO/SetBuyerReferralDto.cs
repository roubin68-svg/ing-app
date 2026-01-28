namespace IngApp.Application.Features.Users.DTO;

/// <summary>
/// DTO برای تنظیم یا تغییر بازاریاب Buyer
/// </summary>
public class SetBuyerReferralDto
{
    /// <summary>
    /// شناسه بازاریاب (اگر از لیست انتخاب شده باشد)
    /// </summary>
    public Guid? ReferredByVisitorId { get; set; }

    /// <summary>
    /// کد معرف بازاریاب (اگر به صورت دستی وارد شده باشد)
    /// </summary>
    public string? ReferralCode { get; set; }
}


