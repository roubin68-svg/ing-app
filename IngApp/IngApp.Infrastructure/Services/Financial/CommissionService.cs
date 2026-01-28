using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Interfaces.Financial;
using IngApp.Application.Features.Financial.DTO;
using IngApp.Domain.Entities.Financial;
using IngApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;

namespace IngApp.Infrastructure.Services.Financial;

public class CommissionService : ICommissionService
{
    private readonly AppDbContext _db;
    private readonly IWalletService _walletService;

    public CommissionService(AppDbContext db, IWalletService walletService)
    {
        _db = db;
        _walletService = walletService;
    }

    public async Task<CommissionResultDto?> ProcessUnlockContactCommissionAsync(
        Guid buyerUserId,
        Guid unlockTransactionId,
        long unlockAmountRial)
    {
        // بررسی اینکه آیا خریدار از طریق بازاریاب معرفی شده است
        var buyerProfile = await _db.BuyerProfiles
            .Include(bp => bp.ReferredByVisitor)
            .FirstOrDefaultAsync(bp => bp.UserId == buyerUserId);

        if (buyerProfile == null || buyerProfile.ReferredByVisitorId == null)
        {
            // خریدار از طریق بازاریاب معرفی نشده است
            return null;
        }

        // ReferredByVisitorId در واقع VisitorProfile.Id است
        var visitorProfileId = buyerProfile.ReferredByVisitorId.Value;
        
        // دریافت VisitorProfile
        var visitorProfile = await _db.VisitorProfiles
            .Include(vp => vp.User)
            .FirstOrDefaultAsync(vp => vp.Id == visitorProfileId);

        if (visitorProfile == null)
        {
            Console.WriteLine($"[Commission] VisitorProfile not found for visitorProfileId: {visitorProfileId}");
            return null;
        }

        if (!visitorProfile.IsActive)
        {
            Console.WriteLine($"[Commission] VisitorProfile {visitorProfileId} is not active");
            return null;
        }

        var visitorUserId = visitorProfile.UserId;

        // بررسی اینکه آیا قبلاً پورسانت پرداخت شده است
        var existingCommission = await _db.CommissionTransactions
            .FirstOrDefaultAsync(ct =>
                ct.VisitorUserId == visitorUserId &&
                ct.ReferenceId == unlockTransactionId &&
                ct.CommissionType == "UnlockContactCommission");

        if (existingCommission != null)
        {
            // پورسانت قبلاً پرداخت شده است
            Console.WriteLine($"[Commission] Commission already processed for unlockTransactionId: {unlockTransactionId}");
            return new CommissionResultDto
            {
                Success = true,
                CommissionTransactionId = existingCommission.Id,
                WalletTransactionId = existingCommission.WalletTransactionId,
                CommissionAmountRial = existingCommission.CommissionAmountRial,
                CommissionPercentage = existingCommission.CommissionPercentage
            };
        }

        // دریافت درصد پورسانت (اول VisitorCommissionRule، اگر نبود از CommissionRule پیش‌فرض)
        Console.WriteLine($"[Commission] Getting commission percentage for visitorProfileId: {visitorProfile.Id}, commissionRuleCode: UnlockContactCommission");
        var (commissionPercentage, commissionRuleId, visitorCommissionRuleId) = await GetCommissionPercentageWithSourceAsync(
            visitorProfile.Id,
            "UnlockContactCommission");

        Console.WriteLine($"[Commission] Commission percentage result: {commissionPercentage}%, CommissionRuleId={commissionRuleId}, VisitorCommissionRuleId={visitorCommissionRuleId}");

        if (commissionPercentage <= 0)
        {
            Console.WriteLine($"[Commission] Commission percentage is 0 or negative for visitorProfileId: {visitorProfile.Id}, commissionRuleCode: UnlockContactCommission");
            return null; // پورسانت صفر است
        }

        // محاسبه پورسانت
        var commissionAmountRial = (long)(unlockAmountRial * commissionPercentage / 100m);

        if (commissionAmountRial <= 0)
        {
            Console.WriteLine($"[Commission] Calculated commission amount is 0 or negative: {commissionAmountRial} Rial");
            return null; // پورسانت صفر است
        }

        // دریافت OperationType و ReferenceType
        var operationType = await _db.FinancialOperationTypes
            .FirstAsync(ot => ot.Code == "CommissionEarned");

        var referenceType = await _db.FinancialReferenceTypes
            .FirstAsync(rt => rt.Code == "WalletTransaction");

        // Credit به Wallet بازاریاب
        var idempotencyKey = $"commission-unlock-{unlockTransactionId}-{visitorUserId}";
        var creditResult = await _walletService.CreditAsync(
            visitorUserId,
            commissionAmountRial,
            operationType.Id,
            referenceType.Id,
            unlockTransactionId,
            idempotencyKey,
            $"پورسانت باز کردن اطلاعات تماس (مبلغ اصلی: {unlockAmountRial / 10m} تومان)");

        if (!creditResult.Success)
        {
            return new CommissionResultDto
            {
                Success = false,
                ErrorMessage = "خطا در پرداخت پورسانت"
            };
        }

        // ثبت Commission Transaction
        var commissionTransaction = new CommissionTransaction
        {
            Id = Guid.NewGuid(),
            VisitorUserId = visitorUserId,
            BuyerUserId = buyerUserId,
            CommissionType = "UnlockContactCommission",
            OriginalAmountRial = unlockAmountRial,
            CommissionAmountRial = commissionAmountRial,
            CommissionPercentage = commissionPercentage,
            CommissionRuleId = commissionRuleId,
            VisitorCommissionRuleId = visitorCommissionRuleId,
            WalletTransactionId = creditResult.TransactionId,
            ReferenceId = unlockTransactionId,
            ReferenceType = "OfferContactUnlock",
            Description = $"پورسانت باز کردن اطلاعات تماس آگهی",
            CreatedAt = DateTime.UtcNow
        };

        _db.CommissionTransactions.Add(commissionTransaction);
        await _db.SaveChangesAsync();

        return new CommissionResultDto
        {
            Success = true,
            CommissionTransactionId = commissionTransaction.Id,
            WalletTransactionId = creditResult.TransactionId,
            CommissionAmountRial = commissionAmountRial,
            CommissionPercentage = commissionPercentage
        };
    }

    public async Task<CommissionResultDto?> ProcessSubscriptionCommissionAsync(
        Guid buyerUserId,
        Guid subscriptionId,
        long subscriptionAmountRial)
    {
        // بررسی اینکه آیا خریدار از طریق بازاریاب معرفی شده است
        var buyerProfile = await _db.BuyerProfiles
            .Include(bp => bp.ReferredByVisitor)
            .FirstOrDefaultAsync(bp => bp.UserId == buyerUserId);

        if (buyerProfile == null)
        {
            Console.WriteLine($"[Commission] BuyerProfile not found for userId: {buyerUserId}");
            return null;
        }

        if (buyerProfile.ReferredByVisitorId == null)
        {
            Console.WriteLine($"[Commission] Buyer {buyerUserId} does not have a referrer (ReferredByVisitorId is null)");
            return null;
        }

        // ReferredByVisitorId در واقع VisitorProfile.Id است
        var visitorProfileId = buyerProfile.ReferredByVisitorId.Value;
        
        // دریافت VisitorProfile
        var visitorProfile = await _db.VisitorProfiles
            .Include(vp => vp.User)
            .FirstOrDefaultAsync(vp => vp.Id == visitorProfileId);

        if (visitorProfile == null || !visitorProfile.IsActive)
        {
            return null; // Visitor معتبر نیست
        }

        var visitorUserId = visitorProfile.UserId;

        // بررسی اینکه آیا قبلاً پورسانت پرداخت شده است
        var existingCommission = await _db.CommissionTransactions
            .FirstOrDefaultAsync(ct =>
                ct.VisitorUserId == visitorUserId &&
                ct.ReferenceId == subscriptionId &&
                ct.CommissionType == "SubscriptionCommission");

        if (existingCommission != null)
        {
            return new CommissionResultDto
            {
                Success = true,
                CommissionTransactionId = existingCommission.Id,
                WalletTransactionId = existingCommission.WalletTransactionId,
                CommissionAmountRial = existingCommission.CommissionAmountRial,
                CommissionPercentage = existingCommission.CommissionPercentage
            };
        }

        // دریافت درصد پورسانت (اول VisitorCommissionRule، اگر نبود از CommissionRule پیش‌فرض)
        Console.WriteLine($"[Commission] Getting commission percentage for visitorProfileId: {visitorProfile.Id}, commissionRuleCode: SubscriptionCommission");
        var (commissionPercentage, commissionRuleId, visitorCommissionRuleId) = await GetCommissionPercentageWithSourceAsync(
            visitorProfile.Id,
            "SubscriptionCommission");

        Console.WriteLine($"[Commission] Commission percentage result: {commissionPercentage}%, CommissionRuleId={commissionRuleId}, VisitorCommissionRuleId={visitorCommissionRuleId}");

        if (commissionPercentage <= 0)
        {
            Console.WriteLine($"[Commission] Commission percentage is 0 or negative for visitorProfileId: {visitorProfile.Id}, commissionRuleCode: SubscriptionCommission");
            return null; // پورسانت صفر است
        }

        // محاسبه پورسانت
        var commissionAmountRial = (long)(subscriptionAmountRial * commissionPercentage / 100m);

        if (commissionAmountRial <= 0)
        {
            Console.WriteLine($"[Commission] Calculated commission amount is 0 or negative: {commissionAmountRial} Rial");
            return null;
        }

        // دریافت OperationType و ReferenceType
        var operationType = await _db.FinancialOperationTypes
            .FirstAsync(ot => ot.Code == "CommissionEarned");

        var referenceType = await _db.FinancialReferenceTypes
            .FirstAsync(rt => rt.Code == "WalletTransaction");

        // Credit به Wallet بازاریاب
        var idempotencyKey = $"commission-subscription-{subscriptionId}-{visitorUserId}";
        var creditResult = await _walletService.CreditAsync(
            visitorUserId,
            commissionAmountRial,
            operationType.Id,
            referenceType.Id,
            subscriptionId,
            idempotencyKey,
            $"پورسانت خرید اشتراک (مبلغ اصلی: {subscriptionAmountRial / 10m} تومان)");

        if (!creditResult.Success)
        {
            return new CommissionResultDto
            {
                Success = false,
                ErrorMessage = "خطا در پرداخت پورسانت"
            };
        }

        // ثبت Commission Transaction
        var commissionTransaction = new CommissionTransaction
        {
            Id = Guid.NewGuid(),
            VisitorUserId = visitorUserId,
            BuyerUserId = buyerUserId,
            CommissionType = "SubscriptionCommission",
            OriginalAmountRial = subscriptionAmountRial,
            CommissionAmountRial = commissionAmountRial,
            CommissionPercentage = commissionPercentage,
            CommissionRuleId = commissionRuleId,
            VisitorCommissionRuleId = visitorCommissionRuleId,
            WalletTransactionId = creditResult.TransactionId,
            ReferenceId = subscriptionId,
            ReferenceType = "UserSubscription",
            Description = $"پورسانت خرید اشتراک",
            CreatedAt = DateTime.UtcNow
        };

        _db.CommissionTransactions.Add(commissionTransaction);
        await _db.SaveChangesAsync();

        return new CommissionResultDto
        {
            Success = true,
            CommissionTransactionId = commissionTransaction.Id,
            WalletTransactionId = creditResult.TransactionId,
            CommissionAmountRial = commissionAmountRial,
            CommissionPercentage = commissionPercentage
        };
    }

    public async Task<List<CommissionTransactionDto>> GetVisitorCommissionsAsync(Guid visitorUserId, int page = 1, int pageSize = 20)
    {
        return await _db.CommissionTransactions
            .Include(ct => ct.BuyerUser)
            .Where(ct => ct.VisitorUserId == visitorUserId)
            .OrderByDescending(ct => ct.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ct => new CommissionTransactionDto
            {
                Id = ct.Id,
                BuyerUserId = ct.BuyerUserId,
                BuyerDisplayName = ct.BuyerUser.DisplayName,
                CommissionType = ct.CommissionType,
                OriginalAmountRial = ct.OriginalAmountRial,
                CommissionAmountRial = ct.CommissionAmountRial,
                CommissionPercentage = ct.CommissionPercentage,
                Description = ct.Description,
                CreatedAt = ct.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<long> GetTotalCommissionAmountAsync(Guid visitorUserId)
    {
        return await _db.CommissionTransactions
            .Where(ct => ct.VisitorUserId == visitorUserId)
            .SumAsync(ct => ct.CommissionAmountRial);
    }

    public async Task<List<CommissionTransactionDto>> GetVisitorCommissionsForAdminAsync(Guid visitorUserId, int page = 1, int pageSize = 20)
    {
        // همان متد قبلی، فقط برای Admin (برای سازگاری)
        return await GetVisitorCommissionsAsync(visitorUserId, page, pageSize);
    }

    public async Task<long> GetTotalCommissionAmountForAdminAsync(Guid visitorUserId)
    {
        // همان متد قبلی، فقط برای Admin (برای سازگاری)
        return await GetTotalCommissionAmountAsync(visitorUserId);
    }

    /// <summary>
    /// دریافت درصد پورسانت برای یک Visitor به‌همراه منبع قانون
    /// اول VisitorCommissionRule را بررسی می‌کند، اگر نبود از CommissionRule پیش‌فرض استفاده می‌کند
    /// </summary>
    private async Task<(decimal Percentage, int? CommissionRuleId, int? VisitorCommissionRuleId)> GetCommissionPercentageWithSourceAsync(
        Guid visitorProfileId,
        string commissionRuleCode)
    {
        var now = DateTime.UtcNow;

        Console.WriteLine($"[Commission] GetCommissionPercentageWithSourceAsync called for visitorProfileId: {visitorProfileId}, commissionRuleCode: {commissionRuleCode}");

        // اول VisitorCommissionRule اختصاصی را بررسی کن
        var visitorRule = await _db.VisitorCommissionRules
            .FirstOrDefaultAsync(vcr =>
                vcr.VisitorProfileId == visitorProfileId &&
                vcr.CommissionRuleCode == commissionRuleCode &&
                vcr.IsActive &&
                (vcr.EffectiveFrom == null || vcr.EffectiveFrom <= now) &&
                (vcr.EffectiveTo == null || vcr.EffectiveTo >= now));

        if (visitorRule != null && visitorRule.CommissionPercentage.HasValue)
        {
            // از Commission اختصاصی Visitor استفاده کن
            Console.WriteLine($"[Commission] Found VisitorCommissionRule for visitorProfileId: {visitorProfileId}, Percentage: {visitorRule.CommissionPercentage.Value}%, Id: {visitorRule.Id}");
            return (visitorRule.CommissionPercentage.Value, null, visitorRule.Id);
        }

        Console.WriteLine($"[Commission] No VisitorCommissionRule found for visitorProfileId: {visitorProfileId}, checking default CommissionRule...");

        // اگر VisitorCommissionRule وجود نداشت یا درصد نداشت، از CommissionRule پیش‌فرض استفاده کن
        var defaultRule = await _db.CommissionRules
            .FirstOrDefaultAsync(cr =>
                cr.Code == commissionRuleCode &&
                cr.IsActive &&
                (cr.EffectiveFrom == null || cr.EffectiveFrom <= now) &&
                (cr.EffectiveTo == null || cr.EffectiveTo >= now));

        if (defaultRule == null)
        {
            Console.WriteLine($"[Commission] ERROR: Default CommissionRule not found for code: {commissionRuleCode}");
            Console.WriteLine($"[Commission] Available CommissionRules in database:");
            var allRules = await _db.CommissionRules.ToListAsync();
            foreach (var rule in allRules)
            {
                Console.WriteLine($"[Commission]   - Code: {rule.Code}, IsActive: {rule.IsActive}, Percentage: {rule.CommissionPercentage}%");
            }
            return (0, null, null); // قانون پورسانت یافت نشد
        }

        Console.WriteLine($"[Commission] Using default CommissionRule: {commissionRuleCode}, Percentage: {defaultRule.CommissionPercentage}%, Id: {defaultRule.Id}");
        return (defaultRule.CommissionPercentage, defaultRule.Id, null);
    }
}

