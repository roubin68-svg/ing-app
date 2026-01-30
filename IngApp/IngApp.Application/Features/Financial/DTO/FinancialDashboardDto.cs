namespace IngApp.Application.Features.Financial.DTO;

/// <summary>
/// داشبورد مالی - خلاصه کلی وضعیت مالی سیستم
/// </summary>
public class FinancialDashboardDto
{
    /// <summary>
    /// درآمد واقعی (پول واقعاً به حساب شرکت آمده) - ریال
    /// شامل: شارژ کیف پول (درگاه) + واریز دستی بانکی
    /// </summary>
    public long TotalRealIncomeRial { get; set; }
    
    /// <summary>
    /// تعداد تراکنش‌های بانکی واریز (درآمد)
    /// </summary>
    public int RealIncomeTransactionCount { get; set; }
    
    /// <summary>
    /// هزینه واقعی (پول واقعاً از حساب شرکت خارج شده) - ریال
    /// شامل: برداشت دستی بانکی
    /// </summary>
    public long TotalRealExpenseRial { get; set; }
    
    /// <summary>
    /// تعداد تراکنش‌های بانکی برداشت (هزینه)
    /// </summary>
    public int RealExpenseTransactionCount { get; set; }
    
    /// <summary>
    /// سود/زیان خالص واقعی - ریال
    /// </summary>
    public long NetRealProfitRial => TotalRealIncomeRial - TotalRealExpenseRial;
    
    /// <summary>
    /// مجموع پورسانت‌های پرداخت شده - ریال
    /// (تراکنش داخلی - به کیف پول بازاریاب واریز شده)
    /// </summary>
    public long TotalCommissionsRial { get; set; }
    
    /// <summary>
    /// تعداد پورسانت‌ها
    /// </summary>
    public int CommissionCount { get; set; }
    
    /// <summary>
    /// مجموع خریدهای اشتراک - ریال
    /// (تراکنش داخلی - از موجودی کیف پول کاربر)
    /// </summary>
    public long TotalSubscriptionPurchasesRial { get; set; }
    
    /// <summary>
    /// تعداد خریدهای اشتراک
    /// </summary>
    public int SubscriptionPurchaseCount { get; set; }
    
    /// <summary>
    /// مجموع باز کردن اطلاعات تماس - ریال
    /// (تراکنش داخلی - از موجودی کیف پول کاربر)
    /// </summary>
    public long TotalUnlockContactFeesRial { get; set; }
    
    /// <summary>
    /// تعداد باز کردن اطلاعات تماس
    /// </summary>
    public int UnlockContactCount { get; set; }
    
    /// <summary>
    /// مجموع موجودی کیف پول‌های کاربران - ریال
    /// </summary>
    public long TotalWalletBalanceRial { get; set; }
    
    /// <summary>
    /// تعداد کاربران دارای کیف پول با تراکنش (گردش حساب)
    /// </summary>
    public int WalletUserWithTransactionCount { get; set; }
    
    /// <summary>
    /// تعداد کل تراکنش‌های بانکی (واقعی) - مجموع واریز و برداشت
    /// </summary>
    public int RealBankTransactionCount => RealIncomeTransactionCount + RealExpenseTransactionCount;
    
    /// <summary>
    /// تعداد کل تراکنش‌های داخلی
    /// </summary>
    public int InternalTransactionCount { get; set; }
}




