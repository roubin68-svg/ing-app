using IngApp.Application.Common.Models;
using IngApp.Application.Features.Financial.DTO;

namespace IngApp.Application.Common.Interfaces.Financial;

/// <summary>
/// Service برای مدیریت Wallet کاربران توسط Admin
/// </summary>
public interface IWalletManagementService
{
    /// <summary>
    /// دریافت لیست کاربران به همراه خلاصه اطلاعات کیف پول
    /// </summary>
    Task<PagedResult<WalletUserSummaryDto>> GetWalletUsersAsync(WalletUserListQueryDto query);

    /// <summary>
    /// دریافت موجودی کیف پول یک کاربر
    /// </summary>
    Task<WalletBalanceDto?> GetUserBalanceAsync(Guid userId);

    /// <summary>
    /// دریافت لیست تراکنش‌های یک کاربر
    /// </summary>
    Task<PagedResult<WalletTransactionDto>> GetUserTransactionsAsync(
        Guid userId,
        int page = 1,
        int pageSize = 20);

    /// <summary>
    /// واریز دستی به کیف پول کاربر (Admin)
    /// </summary>
    Task<WalletTransactionResultDto> ManualDepositAsync(
        Guid userId,
        long amountRial,
        string description);

    /// <summary>
    /// برداشت دستی از کیف پول کاربر (Admin)
    /// </summary>
    Task<WalletTransactionResultDto> ManualWithdrawalAsync(
        Guid userId,
        long amountRial,
        string description);
}

