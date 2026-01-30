namespace IngApp.Application.Features.Financial.DTO;

/// <summary>
/// آیتم گزارش پورسانت‌ها
/// </summary>
public class CommissionReportItemDto
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// شناسه بازاریاب
    /// </summary>
    public Guid VisitorUserId { get; set; }
    
    /// <summary>
    /// شماره موبایل بازاریاب
    /// </summary>
    public string VisitorPhoneNumber { get; set; } = null!;
    
    /// <summary>
    /// نام بازاریاب
    /// </summary>
    public string? VisitorDisplayName { get; set; }
    
    /// <summary>
    /// شناسه خریدار
    /// </summary>
    public Guid BuyerUserId { get; set; }
    
    /// <summary>
    /// شماره موبایل خریدار
    /// </summary>
    public string BuyerPhoneNumber { get; set; } = null!;
    
    /// <summary>
    /// نام خریدار
    /// </summary>
    public string? BuyerDisplayName { get; set; }
    
    /// <summary>
    /// نوع پورسانت
    /// </summary>
    public string CommissionType { get; set; } = null!;
    
    /// <summary>
    /// عنوان نوع پورسانت (فارسی)
    /// </summary>
    public string CommissionTypeTitle { get; set; } = null!;
    
    /// <summary>
    /// مبلغ اصلی تراکنش (ریال)
    /// </summary>
    public long OriginalAmountRial { get; set; }
    
    /// <summary>
    /// مبلغ پورسانت (ریال)
    /// </summary>
    public long CommissionAmountRial { get; set; }
    
    /// <summary>
    /// درصد پورسانت
    /// </summary>
    public decimal CommissionPercentage { get; set; }
    
    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// تاریخ ایجاد
    /// </summary>
    public DateTime CreatedAt { get; set; }
}






