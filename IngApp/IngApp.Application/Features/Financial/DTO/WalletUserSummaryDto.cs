namespace IngApp.Application.Features.Financial.DTO;

public class WalletUserSummaryDto
{
    public Guid UserId { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public string? DisplayName { get; set; }
    public int? UserTypeId { get; set; }
    public string? UserTypeTitle { get; set; }
    public long BalanceRial { get; set; }
}












