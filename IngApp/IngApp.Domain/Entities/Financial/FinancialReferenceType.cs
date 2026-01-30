namespace IngApp.Domain.Entities.Financial;

/// <summary>
/// نوع مرجع تراکنش مالی (Financial Reference Type)
/// مثال: Offer, Subscription, Payment, SupplierOnboarding, WalletTransaction, ...
/// </summary>
public class FinancialReferenceType
{
    public int Id { get; set; }

    /// <summary>
    /// کد یکتا برای شناسایی نوع مرجع (Offer, Subscription, Payment, ...)
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// عنوان قابل نمایش در UI (فارسی)
    /// </summary>
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}





















