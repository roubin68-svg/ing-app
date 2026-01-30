namespace IngApp.Application.Features.Financial.DTO;

public class CommissionResultDto
{
    public bool Success { get; set; }
    public Guid? CommissionTransactionId { get; set; }
    public Guid? WalletTransactionId { get; set; }
    public long CommissionAmountRial { get; set; }
    public decimal CommissionAmountToman => CommissionAmountRial / 10m;
    public decimal CommissionPercentage { get; set; }
    public string? ErrorMessage { get; set; }
}




















