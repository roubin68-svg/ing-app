namespace IngApp.Application.Features.Financial.DTO;

public class UserSubscriptionDetailDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserDisplayName { get; set; } = null!;
    public string UserPhoneNumber { get; set; } = null!;
    public int PlanId { get; set; }
    public string PlanCode { get; set; } = null!;
    public string PlanTitle { get; set; } = null!;
    public int DurationMonths { get; set; }
    public long PlanPriceRial { get; set; }
    public string StatusCode { get; set; } = null!;
    public string StatusTitle { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool UnlimitedContactViews { get; set; }
    public Guid? PaymentTransactionId { get; set; }
    public DateTime PurchasedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime CreatedAt { get; set; }
}










