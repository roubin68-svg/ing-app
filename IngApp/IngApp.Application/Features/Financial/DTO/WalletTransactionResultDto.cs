namespace IngApp.Application.Features.Financial.DTO;

public class WalletTransactionResultDto
{
    public Guid TransactionId { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public long NewBalanceRial { get; set; }
    public decimal NewBalanceToman => NewBalanceRial / 10m;
}





















