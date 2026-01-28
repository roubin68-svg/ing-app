namespace IngApp.Application.Features.Financial.DTO;

public class PaymentVerificationResultDto
{
    public bool Success { get; set; }
    public Guid? PaymentId { get; set; }
    public Guid? WalletTransactionId { get; set; }
    public long? NewBalanceRial { get; set; }
    public decimal? NewBalanceToman => NewBalanceRial.HasValue ? NewBalanceRial.Value / 10m : null;
    public string? ErrorMessage { get; set; }
}










