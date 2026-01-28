using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Interfaces.Financial;
using IngApp.Application.Features.Financial.DTO;
using IngApp.Domain.Entities.Financial;
using IngApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace IngApp.Infrastructure.Services.Financial;

public class SubscriptionService : ISubscriptionService
{
    private readonly AppDbContext _db;
    private readonly IWalletService _walletService;
    private readonly ICommissionService _commissionService;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SubscriptionService(
        AppDbContext db,
        IWalletService walletService,
        ICommissionService commissionService,
        IServiceScopeFactory serviceScopeFactory)
    {
        _db = db;
        _walletService = walletService;
        _commissionService = commissionService;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<List<PlanDto>> GetActivePlansAsync()
    {
        return await _db.Plans
            .Where(p => p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .Select(p => new PlanDto
            {
                Id = p.Id,
                Code = p.Code,
                Title = p.Title,
                Description = p.Description,
                DurationMonths = p.DurationMonths,
                PriceRial = p.PriceRial,
                UnlimitedContactViews = p.UnlimitedContactViews,
                IsActive = p.IsActive,
                DisplayOrder = p.DisplayOrder
            })
            .ToListAsync();
    }

    public async Task<UserSubscriptionDto?> GetActiveSubscriptionAsync(Guid userId)
    {
        var activeStatus = await _db.SubscriptionStatuses
            .FirstAsync(s => s.Code == "Active");

        var now = DateTime.UtcNow;

        var subscription = await _db.UserSubscriptions
            .Include(us => us.Plan)
            .Include(us => us.Status)
            .Where(us =>
                us.UserId == userId &&
                us.StatusId == activeStatus.Id &&
                us.StartDate <= now &&
                us.EndDate >= now)
            .OrderByDescending(us => us.EndDate) // آخرین تاریخ پایان (دیرترین تاریخ)
            .FirstOrDefaultAsync();

        if (subscription == null)
            return null;

        return new UserSubscriptionDto
        {
            Id = subscription.Id,
            PlanId = subscription.PlanId,
            PlanCode = subscription.Plan.Code,
            PlanTitle = subscription.Plan.Title,
            DurationMonths = subscription.Plan.DurationMonths,
            StatusCode = subscription.Status.Code,
            StatusTitle = subscription.Status.Title,
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            UnlimitedContactViews = subscription.Plan.UnlimitedContactViews,
            PaymentTransactionId = subscription.PaymentTransactionId,
            PurchasedAt = subscription.PurchasedAt,
            CancelledAt = subscription.CancelledAt
        };
    }

    /// <summary>
    /// پیدا کردن subscription با آخرین تاریخ پایان (برای منطق non-overlapping)
    /// این subscription می‌تواند در حال حاضر فعال باشد یا هنوز شروع نشده باشد (StartDate در آینده)
    /// </summary>
    private async Task<UserSubscriptionDto?> GetLatestActiveSubscriptionAsync(Guid userId)
    {
        var activeStatus = await _db.SubscriptionStatuses
            .FirstAsync(s => s.Code == "Active");

        // پیدا کردن subscription با آخرین EndDate که Status = Active است
        // چه شروع شده باشد (StartDate <= now) چه هنوز شروع نشده باشد (StartDate > now)
        // اما EndDate باید در آینده باشد (EndDate >= now)
        var now = DateTime.UtcNow;

        var subscription = await _db.UserSubscriptions
            .Include(us => us.Plan)
            .Include(us => us.Status)
            .Where(us =>
                us.UserId == userId &&
                us.StatusId == activeStatus.Id &&
                us.EndDate >= now && // هنوز تمام نشده (یا در آینده تمام می‌شود)
                us.CancelledAt == null) // لغو نشده باشد
            .OrderByDescending(us => us.EndDate) // آخرین تاریخ پایان (دیرترین تاریخ)
            .FirstOrDefaultAsync();

        if (subscription == null)
            return null;

        return new UserSubscriptionDto
        {
            Id = subscription.Id,
            PlanId = subscription.PlanId,
            PlanCode = subscription.Plan.Code,
            PlanTitle = subscription.Plan.Title,
            DurationMonths = subscription.Plan.DurationMonths,
            StatusCode = subscription.Status.Code,
            StatusTitle = subscription.Status.Title,
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            UnlimitedContactViews = subscription.Plan.UnlimitedContactViews,
            PaymentTransactionId = subscription.PaymentTransactionId,
            PurchasedAt = subscription.PurchasedAt,
            CancelledAt = subscription.CancelledAt
        };
    }

    public async Task<bool> HasUnlimitedContactViewsAsync(Guid userId)
    {
        var activeSubscription = await GetActiveSubscriptionAsync(userId);
        return activeSubscription != null && activeSubscription.UnlimitedContactViews;
    }

    public async Task<PurchaseSubscriptionResultDto> PurchaseSubscriptionAsync(Guid userId, int planId, string idempotencyKey)
    {
        // بررسی پلن
        var plan = await _db.Plans
            .FirstOrDefaultAsync(p => p.Id == planId && p.IsActive);

        if (plan == null)
        {
            return new PurchaseSubscriptionResultDto
            {
                Success = false,
                ErrorMessage = "پلن انتخابی یافت نشد یا غیرفعال است."
            };
        }

        // دریافت OperationType و ReferenceType
        var operationType = await _db.FinancialOperationTypes
            .FirstAsync(ot => ot.Code == "SubscriptionPurchase");

        var referenceType = await _db.FinancialReferenceTypes
            .FirstAsync(rt => rt.Code == "Subscription");

        // Debit از Wallet
        var debitResult = await _walletService.DebitAsync(
            userId,
            plan.PriceRial,
            operationType.Id,
            referenceType.Id,
            null, // ReferenceId بعد از ایجاد Subscription تنظیم می‌شود
            idempotencyKey,
            $"خرید اشتراک {plan.Title}");

        if (!debitResult.Success)
        {
            return new PurchaseSubscriptionResultDto
            {
                Success = false,
                ErrorMessage = debitResult.ErrorMessage ?? "خطا در پردازش تراکنش"
            };
        }

        // بررسی اینکه آیا subscription فعال وجود دارد (یا subscription با آخرین EndDate که هنوز شروع نشده)
        // برای منطق non-overlapping، باید subscription با آخرین EndDate را پیدا کنیم (چه شروع شده باشد چه نه)
        var existingActiveSubscription = await GetLatestActiveSubscriptionAsync(userId);

        // ایجاد اشتراک
        var activeStatus = await _db.SubscriptionStatuses
            .FirstAsync(s => s.Code == "Active");

        DateTime startDate;
        DateTime endDate;
        bool willStartAfterActive = false;

        if (existingActiveSubscription != null)
        {
            // اگر subscription فعال وجود دارد، subscription جدید بعد از پایان subscription فعلی شروع می‌شود
            // EndDate شامل است (inclusive)، پس subscription جدید از روز بعد از EndDate شروع می‌شود
            startDate = existingActiveSubscription.EndDate.AddDays(1);
            endDate = CalculateEndDate(startDate, plan.DurationMonths);
            willStartAfterActive = true;
        }
        else
        {
            // اگر subscription فعال وجود ندارد، subscription جدید از الان شروع می‌شود
            startDate = DateTime.UtcNow;
            endDate = CalculateEndDate(startDate, plan.DurationMonths);
        }

        var subscription = new UserSubscription
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PlanId = planId,
            StatusId = activeStatus.Id,
            StartDate = startDate,
            EndDate = endDate,
            PaymentTransactionId = debitResult.TransactionId,
            PurchasedAt = DateTime.UtcNow, // تاریخ خرید همیشه الان است
            CreatedAt = DateTime.UtcNow
        };

        _db.UserSubscriptions.Add(subscription);
        await _db.SaveChangesAsync();

        // به‌روزرسانی ReferenceId در WalletTransaction
        var transaction = await _db.WalletTransactions
            .FirstAsync(t => t.Id == debitResult.TransactionId);
        transaction.ReferenceId = subscription.Id;
        await _db.SaveChangesAsync();

        // پرداخت پورسانت (اگر خریدار از طریق بازاریاب معرفی شده باشد)
        // استفاده از ServiceScopeFactory برای ایجاد DbContext جدید در background task
        Console.WriteLine($"[SubscriptionService] Starting commission processing for subscription {subscription.Id}, userId: {userId}, amount: {plan.PriceRial / 10m} Toman");
        _ = Task.Run(async () =>
        {
            try
            {
                // ایجاد scope جدید برای background task تا DbContext جداگانه داشته باشد
                using var scope = _serviceScopeFactory.CreateScope();
                var commissionService = scope.ServiceProvider.GetRequiredService<ICommissionService>();
                
                Console.WriteLine($"[SubscriptionService] Commission task started for subscription {subscription.Id}");
                var commissionResult = await commissionService.ProcessSubscriptionCommissionAsync(
                    userId,
                    subscription.Id,
                    plan.PriceRial);
                
                if (commissionResult != null && !commissionResult.Success)
                {
                    Console.WriteLine($"[SubscriptionService] Failed to process subscription commission: {commissionResult.ErrorMessage}");
                }
                else if (commissionResult == null)
                {
                    Console.WriteLine($"[SubscriptionService] No commission processed for subscription {subscription.Id} - Buyer may not have a referrer or commission percentage is 0");
                }
                else
                {
                    Console.WriteLine($"[SubscriptionService] Successfully processed subscription commission: {commissionResult.CommissionAmountRial / 10m} Toman ({commissionResult.CommissionPercentage}% of {plan.PriceRial / 10m} Toman)");
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the subscription purchase
                Console.WriteLine($"[SubscriptionService] ERROR processing subscription commission: {ex.Message}");
                Console.WriteLine($"[SubscriptionService] Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[SubscriptionService] Inner exception: {ex.InnerException.Message}");
                }
            }
        });

        return new PurchaseSubscriptionResultDto
        {
            Success = true,
            SubscriptionId = subscription.Id,
            Charged = true,
            ChargedAmountRial = plan.PriceRial,
            TransactionId = debitResult.TransactionId,
            StartDate = startDate,
            EndDate = endDate,
            WillStartAfterActive = willStartAfterActive,
            ActiveSubscriptionEndDate = existingActiveSubscription?.EndDate
        };
    }

    /// <summary>
    /// محاسبه تاریخ پایان subscription با اضافه کردن دقیق تعداد ماه
    /// .NET AddMonths خودش درست handle می‌کند (مثلاً 31 ام در ماه 30 روزه به آخر ماه می‌رود)
    /// 
    /// مثال: اگر StartDate = 18/11 و durationMonths = 1:
    /// - AddMonths(18/11, 1) = 18/12 ✓
    /// - پس EndDate = 18/12 (شامل 18/12)
    /// 
    /// اگر StartDate = 19/11 و durationMonths = 1:
    /// - AddMonths(19/11, 1) = 19/12 ✓
    /// - پس EndDate = 19/12 (شامل 19/12)
    /// </summary>
    private DateTime CalculateEndDate(DateTime startDate, int durationMonths)
    {
        // اضافه کردن دقیق تعداد ماه به تاریخ شروع
        // AddMonths همان روز را در ماه‌های بعدی حفظ می‌کند
        return startDate.AddMonths(durationMonths);
    }

    public async Task<List<UserSubscriptionDto>> GetUserSubscriptionHistoryAsync(Guid userId)
    {
        return await _db.UserSubscriptions
            .Include(us => us.Plan)
            .Include(us => us.Status)
            .Where(us => us.UserId == userId)
            .OrderByDescending(us => us.PurchasedAt)
            .Select(us => new UserSubscriptionDto
            {
                Id = us.Id,
                PlanId = us.PlanId,
                PlanCode = us.Plan.Code,
                PlanTitle = us.Plan.Title,
                DurationMonths = us.Plan.DurationMonths,
                StatusCode = us.Status.Code,
                StatusTitle = us.Status.Title,
                StartDate = us.StartDate,
                EndDate = us.EndDate,
                UnlimitedContactViews = us.Plan.UnlimitedContactViews,
                PaymentTransactionId = us.PaymentTransactionId,
                PurchasedAt = us.PurchasedAt,
                CancelledAt = us.CancelledAt
            })
            .ToListAsync();
    }
}

