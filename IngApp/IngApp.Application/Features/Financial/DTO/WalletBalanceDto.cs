namespace IngApp.Application.Features.Financial.DTO;

public class WalletBalanceDto
{
    public Guid WalletId { get; set; }
    public long BalanceRial { get; set; }
    public decimal BalanceToman => BalanceRial / 10m;
}












