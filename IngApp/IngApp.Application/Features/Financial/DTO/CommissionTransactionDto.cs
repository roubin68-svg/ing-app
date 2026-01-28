namespace IngApp.Application.Features.Financial.DTO;

public class CommissionTransactionDto
{
    public Guid Id { get; set; }
    public Guid BuyerUserId { get; set; }
    public string? BuyerDisplayName { get; set; }
    public string CommissionType { get; set; } = null!;
    public long OriginalAmountRial { get; set; }
    public decimal OriginalAmountToman => OriginalAmountRial / 10m;
    public long CommissionAmountRial { get; set; }
    public decimal CommissionAmountToman => CommissionAmountRial / 10m;
    public decimal CommissionPercentage { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}










