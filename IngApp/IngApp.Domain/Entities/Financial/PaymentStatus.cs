namespace IngApp.Domain.Entities.Financial;

/// <summary>
/// وضعیت پرداخت (Pending, Success, Failed, Cancelled)
/// </summary>
public class PaymentStatus
{
    public int Id { get; set; }
    public string Code { get; set; } = null!; // Pending, Success, Failed, Cancelled
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}










