namespace IngApp.Application.Features.Financial.DTO;

public class CancelSubscriptionResultDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// مبلغ کل اشتراک (ریال)
    /// </summary>
    public long OriginalAmountRial { get; set; }
    
    /// <summary>
    /// تعداد روزهای استفاده شده
    /// </summary>
    public int UsedDays { get; set; }
    
    /// <summary>
    /// تعداد کل روزهای اشتراک
    /// </summary>
    public int TotalDays { get; set; }
    
    /// <summary>
    /// مبلغ کسر شده برای روزهای استفاده شده (ریال)
    /// </summary>
    public long UsedAmountRial { get; set; }
    
    /// <summary>
    /// مبلغ باقیمانده قبل از کسر کارمزد (ریال)
    /// </summary>
    public long RemainingAmountRial { get; set; }
    
    /// <summary>
    /// درصد کارمزد خدمات
    /// </summary>
    public decimal ServiceFeePercentage { get; set; }
    
    /// <summary>
    /// مبلغ کارمزد خدمات (ریال)
    /// </summary>
    public long ServiceFeeAmountRial { get; set; }
    
    /// <summary>
    /// مبلغ نهایی برگشتی به کیف پول (ریال)
    /// </summary>
    public long RefundAmountRial { get; set; }
    
    /// <summary>
    /// شناسه تراکنش واریز به کیف پول
    /// </summary>
    public Guid? RefundTransactionId { get; set; }
    
    /// <summary>
    /// توضیحات کامل محاسبه
    /// </summary>
    public string CalculationDescription { get; set; } = null!;
}



