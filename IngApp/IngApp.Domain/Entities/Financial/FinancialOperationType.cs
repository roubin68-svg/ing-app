namespace IngApp.Domain.Entities.Financial;

/// <summary>
/// نوع عملیات مالی (Financial Operation Type)
/// مثال: UnlockContactFee, SubscriptionPurchase, OnboardingFee, TopUp, CommissionEarned, ...
/// </summary>
public class FinancialOperationType
{
    public int Id { get; set; }

    /// <summary>
    /// کد یکتا برای شناسایی نوع عملیات (UnlockContactFee, SubscriptionPurchase, ...)
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// عنوان قابل نمایش در UI (فارسی)
    /// </summary>
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}












