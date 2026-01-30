namespace IngApp.Domain.Entities.Financial;

/// <summary>
/// درگاه پرداخت (Mock, Zarinpal, etc.)
/// </summary>
public class PaymentGateway
{
    public int Id { get; set; }
    public string Code { get; set; } = null!; // Mock, Zarinpal, etc.
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}




















