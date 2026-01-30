namespace IngApp.Application.Features.Financial.DTO;

/// <summary>
/// DTO برای فیلترهای گزارش پورسانت‌ها
/// </summary>
public class CommissionReportQueryDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    
    /// <summary>
    /// شماره موبایل بازاریاب
    /// </summary>
    public string? VisitorPhoneNumber { get; set; }
    
    /// <summary>
    /// نام بازاریاب
    /// </summary>
    public string? VisitorDisplayName { get; set; }
    
    /// <summary>
    /// نوع پورسانت (UnlockContactCommission, SubscriptionCommission)
    /// </summary>
    public string? CommissionType { get; set; }
    
    /// <summary>
    /// از تاریخ
    /// </summary>
    public DateTime? FromDate { get; set; }
    
    /// <summary>
    /// تا تاریخ
    /// </summary>
    public DateTime? ToDate { get; set; }
}






