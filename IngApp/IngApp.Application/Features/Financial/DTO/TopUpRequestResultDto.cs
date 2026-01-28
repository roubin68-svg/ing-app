namespace IngApp.Application.Features.Financial.DTO;

public class TopUpRequestResultDto
{
    public Guid PaymentId { get; set; }
    public string GatewayCode { get; set; } = null!;
    public string GatewayTitle { get; set; } = null!;
    public long AmountRial { get; set; }
    public decimal AmountToman => AmountRial / 10m;
    public string? RedirectUrl { get; set; } // برای درگاه‌های واقعی
    public string? PaymentToken { get; set; } // برای Mock Gateway
}











