namespace IngApp.Application.Features.Financial.DTO;

/// <summary>
/// آیتم گزارش دفتر کل تراکنش‌های کیف پول
/// </summary>
public class WalletTransactionListItemDto
{
    public Guid TransactionId { get; set; }
    public Guid UserId { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public string? DisplayName { get; set; }

    public string DirectionCode { get; set; } = null!;
    public string DirectionTitle { get; set; } = null!;

    public string OperationTypeCode { get; set; } = null!;
    public string OperationTypeTitle { get; set; } = null!;

    public string StatusCode { get; set; } = null!;
    public string StatusTitle { get; set; } = null!;

    public string ReferenceTypeCode { get; set; } = null!;
    public string ReferenceTypeTitle { get; set; } = null!;

    /// <summary>
    /// دسته منبع تراکنش برای گزارش (Bank / Commission / Manual / Other)
    /// </summary>
    public string SourceCategory { get; set; } = null!;

    public long AmountRial { get; set; }
    public decimal AmountToman => AmountRial / 10m;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
}


