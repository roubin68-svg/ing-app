namespace IngApp.Application.Features.Financial.DTO;

public class WalletUserListQueryDto
{
    public string? PhoneNumber { get; set; }
    public string? DisplayName { get; set; }
    public int? UserTypeId { get; set; }
    public long? MinBalanceRial { get; set; }
    public long? MaxBalanceRial { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}


