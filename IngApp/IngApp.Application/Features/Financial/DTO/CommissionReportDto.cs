using IngApp.Application.Common.Models;

namespace IngApp.Application.Features.Financial.DTO;

/// <summary>
/// گزارش پورسانت‌ها
/// </summary>
public class CommissionReportDto
{
    /// <summary>
    /// لیست پورسانت‌ها (صفحه‌بندی شده)
    /// </summary>
    public PagedResult<CommissionReportItemDto> Commissions { get; set; } = null!;
    
    /// <summary>
    /// مجموع پورسانت‌ها (ریال)
    /// </summary>
    public long TotalCommissionRial { get; set; }
    
    /// <summary>
    /// تعداد کل پورسانت‌ها
    /// </summary>
    public int TotalCount { get; set; }
}






