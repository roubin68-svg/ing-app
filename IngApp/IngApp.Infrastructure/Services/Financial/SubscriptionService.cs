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

        var now = DateTime.Now;

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
        var now = DateTime.Now;

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
            startDate = DateTime.Now;
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
            PurchasedAt = DateTime.Now, // تاریخ خرید همیشه الان است
            CreatedAt = DateTime.Now
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

    public async Task<CancelSubscriptionResultDto> CancelSubscriptionAsync(Guid userId, Guid subscriptionId)
    {
        // دریافت اشتراک
        var subscription = await _db.UserSubscriptions
            .Include(us => us.Plan)
            .Include(us => us.Status)
            .Include(us => us.User)
            .FirstOrDefaultAsync(us => us.Id == subscriptionId && us.UserId == userId);

        if (subscription == null)
        {
            return new CancelSubscriptionResultDto
            {
                Success = false,
                ErrorMessage = "اشتراک مورد نظر یافت نشد."
            };
        }

        // بررسی اینکه اشتراک قبلاً لغو نشده باشد
        if (subscription.CancelledAt.HasValue)
        {
            return new CancelSubscriptionResultDto
            {
                Success = false,
                ErrorMessage = "این اشتراک قبلاً لغو شده است."
            };
        }

        var now = DateTime.Now;
        var originalAmountRial = subscription.Plan.PriceRial;

        // محاسبه تعداد روزهای استفاده شده و کل روزها
        int usedDays = 0;
        int totalDays = 0;
        long usedAmountRial = 0;
        long remainingAmountRial = 0;

        // اگر اشتراک هنوز شروع نشده (StartDate > now)
        if (subscription.StartDate > now)
        {
            // کل مبلغ برگشت داده می‌شود (منهای کارمزد)
            usedDays = 0;
            totalDays = (int)(subscription.EndDate - subscription.StartDate).TotalDays;
            usedAmountRial = 0;
            remainingAmountRial = originalAmountRial;
        }
        // اگر اشتراک شروع شده اما هنوز تمام نشده
        else if (subscription.StartDate <= now && subscription.EndDate > now)
        {
            // محاسبه روزهای استفاده شده
            usedDays = (int)(now - subscription.StartDate).TotalDays;
            totalDays = (int)(subscription.EndDate - subscription.StartDate).TotalDays;
            
            // محاسبه مبلغ استفاده شده (نسبتی)
            if (totalDays > 0)
            {
                usedAmountRial = (long)((decimal)originalAmountRial * usedDays / totalDays);
            }
            else
            {
                usedAmountRial = originalAmountRial; // اگر totalDays = 0، کل مبلغ استفاده شده
            }
            
            remainingAmountRial = originalAmountRial - usedAmountRial;
        }
        // اگر اشتراک تمام شده
        else
        {
            return new CancelSubscriptionResultDto
            {
                Success = false,
                ErrorMessage = "این اشتراک قبلاً منقضی شده و امکان لغو ندارد."
            };
        }

        // دریافت کارمزد خدمات از تنظیمات
        var serviceFeeSetting = await _db.SystemSettings
            .FirstOrDefaultAsync(s => s.Key == "SubscriptionCancellationServiceFeePercentage");

        decimal serviceFeePercentage = 10m; // پیش‌فرض 10%
        if (serviceFeeSetting != null && decimal.TryParse(serviceFeeSetting.Value, out var parsedFee))
        {
            serviceFeePercentage = parsedFee;
        }

        // محاسبه کارمزد خدمات از مبلغ باقیمانده
        long serviceFeeAmountRial = (long)(remainingAmountRial * serviceFeePercentage / 100m);

        // محاسبه مبلغ نهایی برگشتی
        long refundAmountRial = remainingAmountRial - serviceFeeAmountRial;

        // اگر مبلغ برگشتی منفی یا صفر شد، هیچ مبلغی برگشت داده نمی‌شود
        if (refundAmountRial <= 0)
        {
            refundAmountRial = 0;
        }

        // ایجاد تراکنش واریز به کیف پول (اگر مبلغ برگشتی بیشتر از صفر باشد)
        Guid? refundTransactionId = null;
        string calculationDescription = "";

        if (refundAmountRial > 0)
        {
            // دریافت OperationType و ReferenceType
            var operationType = await _db.FinancialOperationTypes
                .FirstAsync(ot => ot.Code == "SubscriptionRefund");

            var referenceType = await _db.FinancialReferenceTypes
                .FirstAsync(rt => rt.Code == "Subscription");

            // ساخت توضیحات کامل محاسبه
            calculationDescription = BuildCalculationDescription(
                originalAmountRial,
                usedDays,
                totalDays,
                usedAmountRial,
                remainingAmountRial,
                serviceFeePercentage,
                serviceFeeAmountRial,
                refundAmountRial,
                subscription.Plan.Title);

            // ایجاد تراکنش واریز
            var idempotencyKey = $"subscription_cancel_{subscriptionId}_{now:yyyyMMddHHmmss}";
            var creditResult = await _walletService.CreditAsync(
                userId,
                refundAmountRial,
                operationType.Id,
                referenceType.Id,
                subscriptionId,
                idempotencyKey,
                calculationDescription);

            if (!creditResult.Success)
            {
                return new CancelSubscriptionResultDto
                {
                    Success = false,
                    ErrorMessage = creditResult.ErrorMessage ?? "خطا در واریز مبلغ به کیف پول."
                };
            }

            refundTransactionId = creditResult.TransactionId;
        }
        else
        {
            // اگر مبلغ برگشتی صفر باشد، فقط توضیحات را می‌سازیم
            calculationDescription = BuildCalculationDescription(
                originalAmountRial,
                usedDays,
                totalDays,
                usedAmountRial,
                remainingAmountRial,
                serviceFeePercentage,
                serviceFeeAmountRial,
                refundAmountRial,
                subscription.Plan.Title);
        }

        // به‌روزرسانی وضعیت اشتراک به Cancelled
        var cancelledStatus = await _db.SubscriptionStatuses
            .FirstAsync(s => s.Code == "Cancelled");
        
        subscription.StatusId = cancelledStatus.Id;
        subscription.CancelledAt = now;
        subscription.UpdatedAt = now;

        await _db.SaveChangesAsync();

        return new CancelSubscriptionResultDto
        {
            Success = true,
            OriginalAmountRial = originalAmountRial,
            UsedDays = usedDays,
            TotalDays = totalDays,
            UsedAmountRial = usedAmountRial,
            RemainingAmountRial = remainingAmountRial,
            ServiceFeePercentage = serviceFeePercentage,
            ServiceFeeAmountRial = serviceFeeAmountRial,
            RefundAmountRial = refundAmountRial,
            RefundTransactionId = refundTransactionId,
            CalculationDescription = calculationDescription
        };
    }

    private string BuildCalculationDescription(
        long originalAmountRial,
        int usedDays,
        int totalDays,
        long usedAmountRial,
        long remainingAmountRial,
        decimal serviceFeePercentage,
        long serviceFeeAmountRial,
        long refundAmountRial,
        string planTitle)
    {
        var originalToman = originalAmountRial / 10m;
        var usedToman = usedAmountRial / 10m;
        var remainingToman = remainingAmountRial / 10m;
        var serviceFeeToman = serviceFeeAmountRial / 10m;
        var refundToman = refundAmountRial / 10m;

        var description = $"برگشت مبلغ اشتراک {planTitle}\n\n";
        
        description += $"مبلغ کل اشتراک: {originalToman:N0} تومان\n";
        
        if (usedDays == 0)
        {
            description += $"وضعیت: اشتراک هنوز شروع نشده است\n";
        }
        else
        {
            description += $"روزهای استفاده شده: {usedDays} روز از {totalDays} روز\n";
            description += $"مبلغ کسر شده برای روزهای استفاده شده: {usedToman:N0} تومان\n";
        }
        
        description += $"مبلغ باقیمانده: {remainingToman:N0} تومان\n";
        description += $"کارمزد خدمات ({serviceFeePercentage}%): {serviceFeeToman:N0} تومان\n";
        description += $"مبلغ نهایی برگشتی: {refundToman:N0} تومان";

        return description;
    }
}

