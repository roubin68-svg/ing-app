namespace IngApp.Application.Features.Financial.DTO;

public class UserSubscriptionDto
{
    public Guid Id { get; set; }
    public int PlanId { get; set; }
    public string PlanCode { get; set; } = null!;
    public string PlanTitle { get; set; } = null!;
    public int DurationMonths { get; set; }
    public long PlanPriceRial { get; set; }
    public string StatusCode { get; set; } = null!;
    public string StatusTitle { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive => StatusCode == "Active" && DateTime.Now >= StartDate && DateTime.Now <= EndDate;
    public bool UnlimitedContactViews { get; set; }
    public Guid? PaymentTransactionId { get; set; }
    public DateTime PurchasedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}












