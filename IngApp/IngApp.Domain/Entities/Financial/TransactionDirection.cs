namespace IngApp.Domain.Entities.Financial;

/// <summary>
/// جهت تراکنش (Transaction Direction)
/// Credit: افزایش موجودی (واریز، پورسانت، ...)
/// Debit: کاهش موجودی (برداشت، هزینه، ...)
/// </summary>
public class TransactionDirection
{
    public int Id { get; set; }

    /// <summary>
    /// کد یکتا برای شناسایی جهت تراکنش (Credit, Debit)
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// عنوان قابل نمایش در UI (فارسی)
    /// </summary>
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}












