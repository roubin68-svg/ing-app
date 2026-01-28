namespace IngApp.Application.Features.Financial.DTO;

public class PaymentStatusDto
{
    public Guid Id { get; set; }
    public string StatusCode { get; set; } = null!;
    public string StatusTitle { get; set; } = null!;
    public string GatewayCode { get; set; } = null!;
    public string GatewayTitle { get; set; } = null!;
    public long AmountRial { get; set; }
    public decimal AmountToman => AmountRial / 10m;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}











