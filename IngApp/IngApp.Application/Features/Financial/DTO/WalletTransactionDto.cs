namespace IngApp.Application.Features.Financial.DTO;

public class WalletTransactionDto
{
    public Guid Id { get; set; }
    public string DirectionCode { get; set; } = null!;
    public string DirectionTitle { get; set; } = null!;
    public long AmountRial { get; set; }
    public decimal AmountToman => AmountRial / 10m;
    public string OperationTypeCode { get; set; } = null!;
    public string OperationTypeTitle { get; set; } = null!;
    public string StatusCode { get; set; } = null!;
    public string StatusTitle { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}











