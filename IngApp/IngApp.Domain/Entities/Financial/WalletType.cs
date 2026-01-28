namespace IngApp.Domain.Entities.Financial;

/// <summary>
/// نوع کیف پول (Wallet Type)
/// در فاز فعلی فقط Main (کیف پول اصلی) پشتیبانی می‌شود.
/// برای آینده: Bonus, Commission, etc.
/// </summary>
public class WalletType
{
    public int Id { get; set; }

    /// <summary>
    /// کد یکتا برای شناسایی نوع کیف پول (Main, Bonus, Commission, ...)
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// عنوان قابل نمایش در UI (فارسی)
    /// </summary>
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}












