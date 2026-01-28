using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Interfaces.Financial;
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Financial.DTO;
using IngApp.Domain.Entities.Financial;
using IngApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IngApp.Infrastructure.Services.Financial;

public class WalletService : IWalletService
{
    private readonly AppDbContext _db;

    public WalletService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Guid> GetOrCreateWalletAsync(Guid userId)
    {
        var wallet = await _db.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId && w.WalletTypeId == 1); // Main wallet

        if (wallet != null)
            return wallet.Id;

        // ایجاد Wallet جدید
        var currency = await _db.Currencies.FirstAsync(c => c.Code == "IRR");
        var walletType = await _db.WalletTypes.FirstAsync(wt => wt.Code == "Main");

        wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CurrencyId = currency.Id,
            WalletTypeId = walletType.Id,
            BalanceRial = 0,
            CreatedAt = DateTime.UtcNow
        };

        _db.Wallets.Add(wallet);
        await _db.SaveChangesAsync();

        return wallet.Id;
    }

    public async Task<WalletBalanceDto?> GetBalanceAsync(Guid userId)
    {
        // اگر Wallet وجود نداشت، خودکار ایجاد می‌کنیم
        var walletId = await GetOrCreateWalletAsync(userId);
        
        var wallet = await _db.Wallets
            .FirstAsync(w => w.Id == walletId);

        return new WalletBalanceDto
        {
            WalletId = wallet.Id,
            BalanceRial = wallet.BalanceRial
        };
    }

    public async Task<WalletTransactionResultDto> CreditAsync(
        Guid userId,
        long amountRial,
        int operationTypeId,
        int referenceTypeId,
        Guid? referenceId,
        string idempotencyKey,
        string? description = null)
    {
        // بررسی Idempotency
        var existingTransaction = await _db.WalletTransactions
            .FirstOrDefaultAsync(t => t.IdempotencyKey == idempotencyKey);

        if (existingTransaction != null)
        {
            var wallet = await _db.Wallets
                .FirstAsync(w => w.Id == existingTransaction.WalletId);

            return new WalletTransactionResultDto
            {
                TransactionId = existingTransaction.Id,
                Success = true,
                NewBalanceRial = wallet.BalanceRial
            };
        }

        // دریافت یا ایجاد Wallet
        var walletId = await GetOrCreateWalletAsync(userId);

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // دریافت Wallet با RowVersion برای Concurrency
            var wallet = await _db.Wallets
                .FirstAsync(w => w.Id == walletId);

            // دریافت Direction (Credit)
            var direction = await _db.TransactionDirections
                .FirstAsync(d => d.Code == "Credit");

            // دریافت Status (Committed)
            var status = await _db.FinancialTransactionStatuses
                .FirstAsync(s => s.Code == "Committed");

            // ایجاد تراکنش
            var walletTransaction = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = walletId,
                DirectionId = direction.Id,
                AmountRial = amountRial,
                OperationTypeId = operationTypeId,
                StatusId = status.Id,
                ReferenceTypeId = referenceTypeId,
                ReferenceId = referenceId,
                IdempotencyKey = idempotencyKey,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            // افزایش Balance
            wallet.BalanceRial += amountRial;

            _db.WalletTransactions.Add(walletTransaction);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return new WalletTransactionResultDto
            {
                TransactionId = walletTransaction.Id,
                Success = true,
                NewBalanceRial = wallet.BalanceRial
            };
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            throw new AppException("خطا در به‌روزرسانی موجودی. لطفاً دوباره تلاش کنید.");
        }
    }

    public async Task<WalletTransactionResultDto> DebitAsync(
        Guid userId,
        long amountRial,
        int operationTypeId,
        int referenceTypeId,
        Guid? referenceId,
        string idempotencyKey,
        string? description = null)
    {
        // 1️⃣ Idempotency check
        var existingTransaction = await _db.WalletTransactions
            .FirstOrDefaultAsync(t => t.IdempotencyKey == idempotencyKey);

        if (existingTransaction != null)
        {
            var existingWallet = await _db.Wallets
                .FirstAsync(w => w.Id == existingTransaction.WalletId);

            return new WalletTransactionResultDto
            {
                TransactionId = existingTransaction.Id,
                Success = true,
                NewBalanceRial = existingWallet.BalanceRial
            };
        }

        // 2️⃣ Get wallet (یا ایجاد کن اگر وجود نداشت)
        var wallet = await _db.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId && w.WalletTypeId == 1);

        if (wallet == null)
        {
            // اگر Wallet وجود نداشت، خودکار ایجاد می‌کنیم
            var walletId = await GetOrCreateWalletAsync(userId);
            wallet = await _db.Wallets.FirstAsync(w => w.Id == walletId);
        }

        // 3️⃣ Balance check (fast fail)
        if (wallet.BalanceRial < amountRial)
        {
            return new WalletTransactionResultDto
            {
                Success = false,
                ErrorMessage = "موجودی کیف پول کافی نیست.",
                NewBalanceRial = wallet.BalanceRial
            };
        }

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // 4️⃣ Reload wallet INSIDE transaction (no redeclare!)
            wallet = await _db.Wallets
                .FirstAsync(w => w.Id == wallet.Id);

            // 5️⃣ Double-check balance (race condition)
            if (wallet.BalanceRial < amountRial)
            {
                await transaction.RollbackAsync();
                return new WalletTransactionResultDto
                {
                    Success = false,
                    ErrorMessage = "موجودی کیف پول کافی نیست.",
                    NewBalanceRial = wallet.BalanceRial
                };
            }

            var direction = await _db.TransactionDirections
                .FirstAsync(d => d.Code == "Debit");

            var status = await _db.FinancialTransactionStatuses
                .FirstAsync(s => s.Code == "Committed");

            var walletTransaction = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                DirectionId = direction.Id,
                AmountRial = amountRial,
                OperationTypeId = operationTypeId,
                StatusId = status.Id,
                ReferenceTypeId = referenceTypeId,
                ReferenceId = referenceId,
                IdempotencyKey = idempotencyKey,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            wallet.BalanceRial -= amountRial;

            _db.WalletTransactions.Add(walletTransaction);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return new WalletTransactionResultDto
            {
                TransactionId = walletTransaction.Id,
                Success = true,
                NewBalanceRial = wallet.BalanceRial
            };
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            throw new AppException("خطا در به‌روزرسانی موجودی. لطفاً دوباره تلاش کنید.");
        }
    }


    public async Task<PagedResult<WalletTransactionDto>> GetTransactionsAsync(
        Guid userId,
        int page = 1,
        int pageSize = 20)
    {
        var wallet = await _db.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId && w.WalletTypeId == 1);

        if (wallet == null)
        {
            return new PagedResult<WalletTransactionDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = 0,
                Items = new List<WalletTransactionDto>()
            };
        }

        var query = _db.WalletTransactions
            .Where(t => t.WalletId == wallet.Id)
            .Include(t => t.Direction)
            .Include(t => t.OperationType)
            .Include(t => t.Status)
            .OrderByDescending(t => t.CreatedAt);

        var totalCount = await query.CountAsync();

        var transactions = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new WalletTransactionDto
            {
                Id = t.Id,
                DirectionCode = t.Direction.Code,
                DirectionTitle = t.Direction.Title,
                AmountRial = t.AmountRial,
                OperationTypeCode = t.OperationType.Code,
                OperationTypeTitle = t.OperationType.Title,
                StatusCode = t.Status.Code,
                StatusTitle = t.Status.Title,
                Description = t.Description,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<WalletTransactionDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = transactions
        };
    }
}


