namespace IngApp.Application.Features.Financial.DTO;

/// <summary>
/// گزارش درآمد/هزینه (فقط تراکنش‌های واقعی بانکی)
/// </summary>
public class IncomeExpenseReportDto
{
    /// <summary>
    /// مجموع درآمدها (ریال)
    /// فقط پول واقعی که به حساب شرکت می‌آید:
    /// - شارژ کیف پول از طریق درگاه (TopUp + Payment)
    /// - واریز دستی بانکی (ManualDeposit + IsBankSettlement)
    /// 
    /// توجه: خرید اشتراک و باز کردن تماس تراکنش‌های داخلی هستند
    /// و نباید به عنوان درآمد محاسبه شوند (کاربر از موجودی کیف پولش استفاده می‌کند)
    /// </summary>
    public long TotalIncomeRial { get; set; }
    
    /// <summary>
    /// مجموع هزینه‌ها (ریال)
    /// فقط پول واقعی که از حساب شرکت خارج می‌شود:
    /// - برداشت دستی بانکی (ManualWithdrawal + IsBankSettlement)
    /// 
    /// توجه: 
    /// - پورسانت‌ها (CommissionEarned) تراکنش داخلی است. پول به کیف پول بازاریاب واریز می‌شود
    ///   اما از حساب بانکی شرکت خارج نمی‌شود. فقط وقتی بازاریاب برداشت کند (ManualWithdrawal + IsBankSettlement)
    ///   آن وقت هزینه واقعی محسوب می‌شود.
    /// - باز کردن تماس و خرید اشتراک تراکنش‌های داخلی هستند
    ///   و نباید به عنوان هزینه محاسبه شوند (کاربر از موجودی کیف پولش استفاده می‌کند)
    /// </summary>
    public long TotalExpenseRial { get; set; }
    
    /// <summary>
    /// سود/زیان خالص (ریال)
    /// </summary>
    public long NetProfitRial => TotalIncomeRial - TotalExpenseRial;
    
    /// <summary>
    /// تعداد تراکنش‌های درآمد
    /// </summary>
    public int IncomeTransactionCount { get; set; }
    
    /// <summary>
    /// تعداد تراکنش‌های هزینه
    /// </summary>
    public int ExpenseTransactionCount { get; set; }
    
    /// <summary>
    /// جزئیات درآمدها (گروه‌بندی شده)
    /// </summary>
    public List<IncomeExpenseCategoryDto> IncomeCategories { get; set; } = new();
    
    /// <summary>
    /// جزئیات هزینه‌ها (گروه‌بندی شده)
    /// </summary>
    public List<IncomeExpenseCategoryDto> ExpenseCategories { get; set; } = new();
}

/// <summary>
/// دسته‌بندی درآمد/هزینه
/// </summary>
public class IncomeExpenseCategoryDto
{
    /// <summary>
    /// نام دسته
    /// </summary>
    public string CategoryName { get; set; } = null!;
    
    /// <summary>
    /// مجموع مبلغ (ریال)
    /// </summary>
    public long TotalAmountRial { get; set; }
    
    /// <summary>
    /// تعداد تراکنش
    /// </summary>
    public int TransactionCount { get; set; }
}

