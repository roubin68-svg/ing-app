using IngApp.Application.Common.Models;
using IngApp.Application.Features.Financial.DTO;

namespace IngApp.Application.Common.Interfaces.Financial;

/// <summary>
/// Service برای مدیریت Wallet و تراکنش‌های مالی
/// </summary>
public interface IWalletService
{
    /// <summary>
    /// ایجاد یا دریافت Wallet برای یک کاربر
    /// </summary>
    Task<Guid> GetOrCreateWalletAsync(Guid userId);

    /// <summary>
    /// دریافت موجودی کیف پول (به ریال و تومان)
    /// </summary>
    Task<WalletBalanceDto?> GetBalanceAsync(Guid userId);

    /// <summary>
    /// Credit (افزایش موجودی)
    /// </summary>
    Task<WalletTransactionResultDto> CreditAsync(
        Guid userId,
        long amountRial,
        int operationTypeId,
        int referenceTypeId,
        Guid? referenceId,
        string idempotencyKey,
        string? description = null);

    /// <summary>
    /// Debit (کاهش موجودی)
    /// </summary>
    Task<WalletTransactionResultDto> DebitAsync(
        Guid userId,
        long amountRial,
        int operationTypeId,
        int referenceTypeId,
        Guid? referenceId,
        string idempotencyKey,
        string? description = null);

    /// <summary>
    /// دریافت لیست تراکنش‌های یک کاربر
    /// </summary>
    Task<PagedResult<WalletTransactionDto>> GetTransactionsAsync(
        Guid userId,
        int page = 1,
        int pageSize = 20);
}

