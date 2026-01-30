using IngApp.Application.Common.Models;

namespace IngApp.Application.Features.Financial.DTO;

/// <summary>
/// خروجی گزارش دفتر کل تراکنش‌های کیف پول
/// </summary>
public class WalletTransactionsReportDto
{
    /// <summary>
    /// لیست صفحه‌بندی‌شده تراکنش‌ها
    /// </summary>
    public PagedResult<WalletTransactionListItemDto> Transactions { get; set; } = null!;

    /// <summary>
    /// جمع کل مبالغ تراکنش‌های Credit (واریز به کیف پول) بر اساس فیلتر
    /// </summary>
    public long TotalCreditRial { get; set; }

    /// <summary>
    /// جمع کل مبالغ تراکنش‌های Debit (برداشت از کیف پول) بر اساس فیلتر
    /// </summary>
    public long TotalDebitRial { get; set; }
}











