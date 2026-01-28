namespace IngApp.Domain.Entities.Users;

/// <summary>
/// پروفایل بازاریاب (Visitor)
/// هر کاربر می‌تواند یک VisitorProfile داشته باشد
/// </summary>
public class VisitorProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    /// <summary>
    /// کد معرف (برای لینک‌های ارجاع)
    /// </summary>
    public string ReferralCode { get; set; } = null!;
    
    /// <summary>
    /// نام کسب‌وکار یا نام نمایشی
    /// </summary>
    public string? BusinessName { get; set; }
    
    /// <summary>
    /// شماره موبایل (ممکن است با User.PhoneNumber متفاوت باشد)
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
    /// فعال/غیرفعال
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}











