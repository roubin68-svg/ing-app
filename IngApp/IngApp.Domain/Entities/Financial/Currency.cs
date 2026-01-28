namespace IngApp.Domain.Entities.Financial;

/// <summary>
/// واحد پول (Currency)
/// در فاز فعلی فقط IRR (ریال ایران) پشتیبانی می‌شود.
/// </summary>
public class Currency
{
    public int Id { get; set; }

    /// <summary>
    /// کد یکتا برای شناسایی واحد پول (IRR, USD, EUR, ...)
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// عنوان قابل نمایش در UI (فارسی)
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// نماد واحد پول (ریال، دلار، ...)
    /// </summary>
    public string? Symbol { get; set; }

    /// <summary>
    /// نرخ تبدیل به ریال (برای واحدهای غیر ریال)
    /// برای IRR = 1
    /// </summary>
    public decimal ExchangeRateToRial { get; set; } = 1;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}











