namespace IngApp.Domain.Entities.Financial;

/// <summary>
/// تراکنش کیف پول (Ledger Entry)
/// هر تغییر در Balance باید با یک WalletTransaction ثبت شود.
/// تمام عملیات‌های مالی باید Idempotent باشند (با IdempotencyKey).
/// </summary>
public class WalletTransaction
{
    public Guid Id { get; set; }

    /// <summary>
    /// کیف پول مربوطه
    /// </summary>
    public Guid WalletId { get; set; }
    public Wallet Wallet { get; set; } = null!;

    /// <summary>
    /// جهت تراکنش (Credit/Debit)
    /// </summary>
    public int DirectionId { get; set; }
    public TransactionDirection Direction { get; set; } = null!;

    /// <summary>
    /// مبلغ تراکنش به ریال (> 0)
    /// </summary>
    public long AmountRial { get; set; }

    /// <summary>
    /// نوع عملیات مالی (UnlockContactFee, SubscriptionPurchase, ...)
    /// </summary>
    public int OperationTypeId { get; set; }
    public FinancialOperationType OperationType { get; set; } = null!;

    /// <summary>
    /// وضعیت تراکنش (Pending, Committed, Failed, Reversed)
    /// </summary>
    public int StatusId { get; set; }
    public FinancialTransactionStatus Status { get; set; } = null!;

    /// <summary>
    /// نوع مرجع تراکنش (Offer, Subscription, Payment, ...)
    /// </summary>
    public int ReferenceTypeId { get; set; }
    public FinancialReferenceType ReferenceType { get; set; } = null!;

    /// <summary>
    /// شناسه مرجع (مثلاً OfferId, SubscriptionId, PaymentId, ...)
    /// </summary>
    public Guid? ReferenceId { get; set; }

    /// <summary>
    /// کلید یکتا برای جلوگیری از تکرار تراکنش (Idempotency)
    /// </summary>
    public string IdempotencyKey { get; set; } = null!;

    /// <summary>
    /// توضیحات تراکنش
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// اگر true باشد یعنی این تراکنش برداشت/واریز مربوط به تسویهٔ واقعی با کاربر (مثلاً انتقال به حساب بانکی) است
    /// و در گزارش‌ها به عنوان تراکنش «بانکی» نمایش داده می‌شود.
    /// پیش‌فرض: false (تغییرات داخلی/سیستمی)
    /// </summary>
    public bool IsBankSettlement { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}













