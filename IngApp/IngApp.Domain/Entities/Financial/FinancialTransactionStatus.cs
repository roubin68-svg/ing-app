namespace IngApp.Domain.Entities.Financial;

/// <summary>
/// وضعیت تراکنش مالی (Financial Transaction Status)
/// Pending, Committed, Failed, Reversed
/// </summary>
public class FinancialTransactionStatus
{
    public int Id { get; set; }

    /// <summary>
    /// کد یکتا برای شناسایی وضعیت (Pending, Committed, Failed, Reversed)
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// عنوان قابل نمایش در UI (فارسی)
    /// </summary>
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}





















