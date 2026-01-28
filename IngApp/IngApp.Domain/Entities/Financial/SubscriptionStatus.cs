namespace IngApp.Domain.Entities.Financial;

/// <summary>
/// وضعیت اشتراک (Active, Expired, Cancelled, Pending)
/// </summary>
public class SubscriptionStatus
{
    public int Id { get; set; }
    public string Code { get; set; } = null!; // Active, Expired, Cancelled, Pending
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}










