using IngApp.Domain.Entities.Financial;

namespace IngApp.Domain.Entities.Offers;

/// <summary>
/// ثبت Unlock اطلاعات تماس یک آگهی برای یک کاربر
/// هر کاربر برای هر آگهی فقط یک‌بار Unlock می‌کند (Unique: OfferId, UserId)
/// </summary>
public class OfferContactUnlock
{
    public Guid Id { get; set; }

    /// <summary>
    /// آگهی مربوطه
    /// </summary>
    public int OfferId { get; set; }
    public Offer Offer { get; set; } = null!;

    /// <summary>
    /// کاربری که Contact را Unlock کرده
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// تاریخ Unlock
    /// </summary>
    public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// تراکنش شارژ شده (اگر از Wallet پرداخت شده باشد)
    /// null اگر از طریق Subscription بوده باشد
    /// </summary>
    public Guid? ChargedTransactionId { get; set; }

    /// <summary>
    /// نوع منبع Unlock (Paid یا Subscription)
    /// </summary>
    public int SourceTypeId { get; set; }
    public UnlockSourceType SourceType { get; set; } = null!;
}











