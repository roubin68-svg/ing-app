namespace IngApp.Application.Features.Users.DTO;

/// <summary>
/// DTO برای ایجاد Buyer جدید توسط Admin
/// </summary>
public class CreateBuyerDto
{
    /// <summary>
    /// شماره موبایل (اگر User وجود نداشته باشد، ایجاد می‌شود)
    /// </summary>
    public string PhoneNumber { get; set; } = null!;

    /// <summary>
    /// نام نمایشی (اختیاری)
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// نام کسب‌وکار
    /// </summary>
    public string? BusinessName { get; set; }

    /// <summary>
    /// شماره تماس
    /// </summary>
    public string? ContactMobile { get; set; }

    /// <summary>
    /// استان
    /// </summary>
    public string? Province { get; set; }

    /// <summary>
    /// شهر
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// آدرس
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// شناسه بازاریاب (اگر از لیست انتخاب شده باشد)
    /// </summary>
    public Guid? ReferredByVisitorId { get; set; }

    /// <summary>
    /// کد معرف بازاریاب (اگر به صورت دستی وارد شده باشد)
    /// </summary>
    public string? ReferralCode { get; set; }
}



