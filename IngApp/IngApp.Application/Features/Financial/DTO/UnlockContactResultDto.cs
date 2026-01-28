namespace IngApp.Application.Features.Financial.DTO;

public class UnlockContactResultDto
{
    public bool IsUnlocked { get; set; }
    public bool Charged { get; set; }
    public long? ChargedAmountRial { get; set; }
    public decimal? ChargedAmountToman => ChargedAmountRial.HasValue ? ChargedAmountRial.Value / 10m : null;
    public string? ErrorMessage { get; set; }
    public Guid? TransactionId { get; set; }
}












