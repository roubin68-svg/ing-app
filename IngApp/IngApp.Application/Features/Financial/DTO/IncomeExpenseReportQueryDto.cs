namespace IngApp.Application.Features.Financial.DTO;

/// <summary>
/// DTO برای فیلترهای گزارش درآمد/هزینه
/// </summary>
public class IncomeExpenseReportQueryDto
{
    /// <summary>
    /// از تاریخ
    /// </summary>
    public DateTime? FromDate { get; set; }
    
    /// <summary>
    /// تا تاریخ
    /// </summary>
    public DateTime? ToDate { get; set; }
}






