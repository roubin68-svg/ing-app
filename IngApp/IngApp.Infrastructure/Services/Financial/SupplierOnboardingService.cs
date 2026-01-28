using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Interfaces.Financial;
using IngApp.Application.Features.Financial.DTO;
using IngApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IngApp.Infrastructure.Services.Financial;

public class SupplierOnboardingService : ISupplierOnboardingService
{
    private readonly AppDbContext _db;
    private readonly IWalletService _walletService;

    public SupplierOnboardingService(AppDbContext db, IWalletService walletService)
    {
        _db = db;
        _walletService = walletService;
    }

    public async Task<bool> HasPaidOnboardingFeeAsync(Guid userId)
    {
        // بررسی اینکه آیا تراکنش موفق برای Onboarding Fee وجود دارد
        var referenceType = await _db.FinancialReferenceTypes
            .FirstOrDefaultAsync(rt => rt.Code == "SupplierOnboarding");

        if (referenceType == null)
            return false;

        // دریافت Wallet کاربر
        var wallet = await _db.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId && w.WalletTypeId == 1); // Main wallet

        if (wallet == null)
            return false;

        // بررسی اینکه آیا تراکنش موفق (Committed) برای Onboarding Fee وجود دارد
        var committedStatus = await _db.FinancialTransactionStatuses
            .FirstAsync(s => s.Code == "Committed");

        var debitDirection = await _db.TransactionDirections
            .FirstAsync(d => d.Code == "Debit");

        var hasPaid = await _db.WalletTransactions
            .AnyAsync(t =>
                t.WalletId == wallet.Id &&
                t.ReferenceTypeId == referenceType.Id &&
                t.ReferenceId == userId && // ReferenceId = UserId برای Onboarding
                t.StatusId == committedStatus.Id &&
                t.DirectionId == debitDirection.Id);

        return hasPaid;
    }

    public async Task<SupplierOnboardingResultDto> PayOnboardingFeeAsync(Guid userId, string idempotencyKey)
    {
        // بررسی اینکه آیا قبلاً پرداخت شده است
        var hasPaid = await HasPaidOnboardingFeeAsync(userId);

        if (hasPaid)
        {
            // پیدا کردن تراکنش قبلی
            var onboardingReferenceType = await _db.FinancialReferenceTypes
                .FirstAsync(rt => rt.Code == "SupplierOnboarding");

            var wallet = await _db.Wallets
                .FirstAsync(w => w.UserId == userId && w.WalletTypeId == 1);

            var committedStatus = await _db.FinancialTransactionStatuses
                .FirstAsync(s => s.Code == "Committed");

            var debitDirection = await _db.TransactionDirections
                .FirstAsync(d => d.Code == "Debit");

            var previousTransaction = await _db.WalletTransactions
                .Where(t =>
                    t.WalletId == wallet.Id &&
                    t.ReferenceTypeId == onboardingReferenceType.Id &&
                    t.ReferenceId == userId &&
                    t.StatusId == committedStatus.Id &&
                    t.DirectionId == debitDirection.Id)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync();

            return new SupplierOnboardingResultDto
            {
                HasPaid = true,
                Charged = false,
                TransactionId = previousTransaction?.Id
            };
        }

        // دریافت تعرفه OnboardingFee
        var pricing = await _db.Pricings
            .FirstOrDefaultAsync(p => p.Code == "OnboardingFee" && p.IsActive);

        if (pricing == null)
        {
            throw new AppException("تعرفه هزینه ثبت‌نام تأمین‌کننده یافت نشد.");
        }

        // دریافت OperationType و ReferenceType
        var operationType = await _db.FinancialOperationTypes
            .FirstAsync(ot => ot.Code == "OnboardingFee");

        var referenceType = await _db.FinancialReferenceTypes
            .FirstAsync(rt => rt.Code == "SupplierOnboarding");

        // Debit از Wallet
        var debitResult = await _walletService.DebitAsync(
            userId,
            pricing.AmountRial,
            operationType.Id,
            referenceType.Id,
            userId, // ReferenceId = UserId برای Onboarding
            idempotencyKey,
            "هزینه ثبت‌نام تأمین‌کننده");

        if (!debitResult.Success)
        {
            return new SupplierOnboardingResultDto
            {
                HasPaid = false,
                Charged = false,
                ErrorMessage = debitResult.ErrorMessage ?? "خطا در پردازش تراکنش"
            };
        }

        return new SupplierOnboardingResultDto
        {
            HasPaid = true,
            Charged = true,
            ChargedAmountRial = pricing.AmountRial,
            TransactionId = debitResult.TransactionId
        };
    }
}

