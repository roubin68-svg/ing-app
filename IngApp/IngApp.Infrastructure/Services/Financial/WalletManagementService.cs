using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Interfaces.Financial;
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Financial.DTO;
using IngApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IngApp.Infrastructure.Services.Financial;

public class WalletManagementService : IWalletManagementService
{
    private readonly AppDbContext _db;
    private readonly IWalletService _walletService;

    public WalletManagementService(AppDbContext db, IWalletService walletService)
    {
        _db = db;
        _walletService = walletService;
    }

        public async Task<PagedResult<WalletUserSummaryDto>> GetWalletUsersAsync(WalletUserListQueryDto query)
        {
            var usersQuery =
                from u in _db.Users.AsNoTracking()
                join ut in _db.UserTypes.AsNoTracking() on u.UserTypeId equals ut.Id into utJoin
                from ut in utJoin.DefaultIfEmpty()
                join w in _db.Wallets.AsNoTracking().Where(w => w.WalletTypeId == 1) on u.Id equals w.UserId into wJoin
                from w in wJoin.DefaultIfEmpty()
                select new
                {
                    u.Id,
                    u.PhoneNumber,
                    u.DisplayName,
                    u.UserTypeId,
                    UserTypeTitle = ut != null ? ut.Title : null,
                    BalanceRial = w != null ? w.BalanceRial : 0
                };

            if (!string.IsNullOrWhiteSpace(query.PhoneNumber))
            {
                usersQuery = usersQuery.Where(x => x.PhoneNumber.Contains(query.PhoneNumber));
            }

            if (!string.IsNullOrWhiteSpace(query.DisplayName))
            {
                usersQuery = usersQuery.Where(x => x.DisplayName != null && x.DisplayName.Contains(query.DisplayName));
            }

            if (query.UserTypeId.HasValue)
            {
                usersQuery = usersQuery.Where(x => x.UserTypeId == query.UserTypeId.Value);
            }

            if (query.MinBalanceRial.HasValue)
            {
                usersQuery = usersQuery.Where(x => x.BalanceRial >= query.MinBalanceRial.Value);
            }

            if (query.MaxBalanceRial.HasValue)
            {
                usersQuery = usersQuery.Where(x => x.BalanceRial <= query.MaxBalanceRial.Value);
            }

            var page = query.Page <= 0 ? 1 : query.Page;
            var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;

            var totalCount = await usersQuery.CountAsync();

            var items = await usersQuery
                .OrderBy(x => x.PhoneNumber)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new WalletUserSummaryDto
                {
                    UserId = x.Id,
                    PhoneNumber = x.PhoneNumber,
                    DisplayName = x.DisplayName,
                    UserTypeId = x.UserTypeId,
                    UserTypeTitle = x.UserTypeTitle,
                    BalanceRial = x.BalanceRial
                })
                .ToListAsync();

            return new PagedResult<WalletUserSummaryDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = items
            };
        }

    public async Task<WalletBalanceDto?> GetUserBalanceAsync(Guid userId)
    {
        return await _walletService.GetBalanceAsync(userId);
    }

    public async Task<PagedResult<WalletTransactionDto>> GetUserTransactionsAsync(
        Guid userId,
        int page = 1,
        int pageSize = 20)
    {
        return await _walletService.GetTransactionsAsync(userId, page, pageSize);
    }

    public async Task<WalletTransactionResultDto> ManualDepositAsync(
        Guid userId,
        long amountRial,
        string description)
    {
        if (amountRial <= 0)
            throw new ValidationException(new() { "مبلغ واریز باید بیشتر از صفر باشد." });

        // بررسی وجود کاربر
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            throw new NotFoundException("کاربر یافت نشد.");

        // دریافت OperationType و ReferenceType
        var operationType = await _db.FinancialOperationTypes
            .FirstOrDefaultAsync(ot => ot.Code == "ManualDeposit");

        if (operationType == null)
            throw new AppException("نوع عملیات 'ManualDeposit' یافت نشد. لطفاً با مدیر سیستم تماس بگیرید.");

        var referenceType = await _db.FinancialReferenceTypes
            .FirstOrDefaultAsync(rt => rt.Code == "AdminAction");

        if (referenceType == null)
            throw new AppException("نوع مرجع 'AdminAction' یافت نشد. لطفاً با مدیر سیستم تماس بگیرید.");

        // Credit به Wallet
        // IdempotencyKey حداکثر 100 کاراکتر است، پس الگو را کوتاه نگه می‌داریم
        var idempotencyKey = $"manual-deposit-{userId}-{DateTime.UtcNow:yyyyMMddHHmmss}";
        var creditResult = await _walletService.CreditAsync(
            userId,
            amountRial,
            operationType.Id,
            referenceType.Id,
            null,
            idempotencyKey,
            !string.IsNullOrWhiteSpace(description) ? description.Trim() : "واریز دستی توسط مدیر");

        if (!creditResult.Success)
            throw new AppException(creditResult.ErrorMessage ?? "خطا در واریز وجه");

        return creditResult;
    }

    public async Task<WalletTransactionResultDto> ManualWithdrawalAsync(
        Guid userId,
        long amountRial,
        string description)
    {
        if (amountRial <= 0)
            throw new ValidationException(new() { "مبلغ برداشت باید بیشتر از صفر باشد." });

        // بررسی وجود کاربر
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            throw new NotFoundException("کاربر یافت نشد.");

        // بررسی موجودی کافی
        var balance = await _walletService.GetBalanceAsync(userId);
        if (balance == null || balance.BalanceRial < amountRial)
            throw new ValidationException(new() { "موجودی کیف پول کافی نیست." });

        // دریافت OperationType و ReferenceType
        var operationType = await _db.FinancialOperationTypes
            .FirstOrDefaultAsync(ot => ot.Code == "ManualWithdrawal");

        if (operationType == null)
            throw new AppException("نوع عملیات 'ManualWithdrawal' یافت نشد. لطفاً با مدیر سیستم تماس بگیرید.");

        var referenceType = await _db.FinancialReferenceTypes
            .FirstOrDefaultAsync(rt => rt.Code == "AdminAction");

        if (referenceType == null)
            throw new AppException("نوع مرجع 'AdminAction' یافت نشد. لطفاً با مدیر سیستم تماس بگیرید.");

        // Debit از Wallet
        // IdempotencyKey حداکثر 100 کاراکتر است، پس الگو را کوتاه نگه می‌داریم
        var idempotencyKey = $"manual-withdrawal-{userId}-{DateTime.UtcNow:yyyyMMddHHmmss}";
        var debitResult = await _walletService.DebitAsync(
            userId,
            amountRial,
            operationType.Id,
            referenceType.Id,
            null,
            idempotencyKey,
            !string.IsNullOrWhiteSpace(description) ? description.Trim() : "برداشت دستی توسط مدیر");

        if (!debitResult.Success)
            throw new AppException(debitResult.ErrorMessage ?? "خطا در برداشت وجه");

        return debitResult;
    }
}

