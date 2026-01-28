namespace IngApp.Application.Features.Financial.DTO;

public class PurchaseSubscriptionResultDto
{
    public bool Success { get; set; }
    public Guid? SubscriptionId { get; set; }
    public bool Charged { get; set; }
    public long? ChargedAmountRial { get; set; }
    public decimal? ChargedAmountToman => ChargedAmountRial.HasValue ? ChargedAmountRial.Value / 10m : null;
    public string? ErrorMessage { get; set; }
    public Guid? TransactionId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// آیا subscription جدید بعد از subscription فعلی شروع می‌شود؟
    /// </summary>
    public bool WillStartAfterActive { get; set; }
    
    /// <summary>
    /// تاریخ پایان subscription فعلی (اگر وجود داشته باشد)
    /// </summary>
    public DateTime? ActiveSubscriptionEndDate { get; set; }
}

