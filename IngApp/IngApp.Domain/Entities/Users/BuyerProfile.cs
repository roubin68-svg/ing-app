namespace IngApp.Domain.Entities.Users;

/// <summary>
/// پروفایل خریدار
/// هر کاربر می‌تواند یک BuyerProfile داشته باشد
/// </summary>
public class BuyerProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
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
    /// شناسه معرف (Visitor) - اگر از طریق بازاریاب معرفی شده باشد
    /// </summary>
    public Guid? ReferredByVisitorId { get; set; }
    public VisitorProfile? ReferredByVisitor { get; set; }
    
    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}










