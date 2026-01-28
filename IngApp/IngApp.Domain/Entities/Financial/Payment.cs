using IngApp.Domain.Entities.Users;

namespace IngApp.Domain.Entities.Financial;

/// <summary>
/// تراکنش پرداخت (برای TopUp و سایر پرداخت‌ها)
/// </summary>
public class Payment
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public int GatewayId { get; set; }
    public PaymentGateway Gateway { get; set; } = null!;
    
    public int StatusId { get; set; }
    public PaymentStatus Status { get; set; } = null!;
    
    /// <summary>
    /// مبلغ به ریال
    /// </summary>
    public long AmountRial { get; set; }
    
    /// <summary>
    /// شناسه تراکنش در درگاه پرداخت
    /// </summary>
    public string? GatewayTransactionId { get; set; }
    
    /// <summary>
    /// شناسه تراکنش Wallet (بعد از موفقیت پرداخت)
    /// </summary>
    public Guid? WalletTransactionId { get; set; }
    
    /// <summary>
    /// کلید Idempotency برای جلوگیری از تراکنش‌های تکراری
    /// </summary>
    public string IdempotencyKey { get; set; } = null!;
    
    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// اطلاعات اضافی از درگاه پرداخت (JSON)
    /// </summary>
    public string? GatewayResponseJson { get; set; }
    
    /// <summary>
    /// تاریخ ایجاد
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// تاریخ به‌روزرسانی
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// تاریخ تکمیل (موفق یا ناموفق)
    /// </summary>
    public DateTime? CompletedAt { get; set; }
}











