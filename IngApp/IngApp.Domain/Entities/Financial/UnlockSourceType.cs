namespace IngApp.Domain.Entities.Financial;

/// <summary>
/// نوع منبع Unlock (Unlock Source Type)
/// Paid: پرداخت شده (از Wallet)
/// Subscription: از طریق اشتراک فعال
/// </summary>
public class UnlockSourceType
{
    public int Id { get; set; }

    /// <summary>
    /// کد یکتا برای شناسایی نوع منبع (Paid, Subscription)
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// عنوان قابل نمایش در UI (فارسی)
    /// </summary>
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}












