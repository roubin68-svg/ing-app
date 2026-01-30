using IngApp.Application.Common.Interfaces.Financial;
using IngApp.Application.Common.Models;
using IngApp.Application.Common.Exceptions;
using IngApp.Application.Features.Financial.DTO;
using IngApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using IngApp.Domain.Entities.Financial;

namespace IngApp.Infrastructure.Services.Financial;

public class UserSubscriptionManagementService : IUserSubscriptionManagementService
{
    private readonly AppDbContext _db;
    private readonly IWalletService _walletService;

    public UserSubscriptionManagementService(AppDbContext db, IWalletService walletService)
    {
        _db = db;
        _walletService = walletService;
    }

    public async Task<PagedResult<UserSubscriptionDetailDto>> GetPagedSubscriptionsAsync(UserSubscriptionListQueryDto query)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;

        var subscriptionsQuery = _db.UserSubscriptions
            .AsNoTracking()
            .Include(us => us.User)
            .Include(us => us.Plan)
            .Include(us => us.Status)
            .AsQueryable();

        // فیلتر بر اساس UserId
        if (query.UserId.HasValue)
        {
            subscriptionsQuery = subscriptionsQuery.Where(us => us.UserId == query.UserId.Value);
        }

        // فیلتر بر اساس StatusCode
        if (!string.IsNullOrWhiteSpace(query.StatusCode))
        {
            subscriptionsQuery = subscriptionsQuery.Where(us => us.Status.Code == query.StatusCode);
        }

        // فیلتر بر اساس PlanId
        if (query.PlanId.HasValue)
        {
            subscriptionsQuery = subscriptionsQuery.Where(us => us.PlanId == query.PlanId.Value);
        }

        // فیلتر بر اساس شماره موبایل کاربر
        if (!string.IsNullOrWhiteSpace(query.UserPhoneNumber))
        {
            subscriptionsQuery = subscriptionsQuery.Where(us => 
                us.User.PhoneNumber.Contains(query.UserPhoneNumber));
        }

        // فیلتر بر اساس نام کاربر
        if (!string.IsNullOrWhiteSpace(query.UserDisplayName))
        {
            subscriptionsQuery = subscriptionsQuery.Where(us => 
                us.User.DisplayName != null && us.User.DisplayName.Contains(query.UserDisplayName));
        }

        var totalCount = await subscriptionsQuery.CountAsync();

        var subscriptions = await subscriptionsQuery
            .OrderByDescending(us => us.PurchasedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(us => new UserSubscriptionDetailDto
            {
                Id = us.Id,
                UserId = us.UserId,
                UserDisplayName = us.User.DisplayName ?? "بدون نام",
                UserPhoneNumber = us.User.PhoneNumber,
                PlanId = us.PlanId,
                PlanCode = us.Plan.Code,
                PlanTitle = us.Plan.Title,
                DurationMonths = us.Plan.DurationMonths,
                PlanPriceRial = us.Plan.PriceRial,
                StatusCode = us.Status.Code,
                StatusTitle = us.Status.Title,
                StartDate = us.StartDate,
                EndDate = us.EndDate,
                UnlimitedContactViews = us.Plan.UnlimitedContactViews,
                PaymentTransactionId = us.PaymentTransactionId,
                PurchasedAt = us.PurchasedAt,
                CancelledAt = us.CancelledAt,
                CreatedAt = us.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<UserSubscriptionDetailDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = subscriptions
        };
    }

    public async Task<PagedResult<UserWithSubscriptionsSummaryDto>> GetUsersWithSubscriptionsSummaryAsync(UsersWithSubscriptionsQueryDto query)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;

        // دریافت کاربرانی که اشتراک خریداری کرده‌اند
        var usersWithSubscriptionsQuery = _db.UserSubscriptions
            .AsNoTracking()
            .Include(us => us.User)
            .Include(us => us.Plan)
            .Include(us => us.Status)
            .AsQueryable();

        // فیلتر بر اساس شماره موبایل کاربر
        if (!string.IsNullOrWhiteSpace(query.UserPhoneNumber))
        {
            usersWithSubscriptionsQuery = usersWithSubscriptionsQuery.Where(us =>
                us.User.PhoneNumber.Contains(query.UserPhoneNumber));
        }

        // فیلتر بر اساس نام کاربر
        if (!string.IsNullOrWhiteSpace(query.UserDisplayName))
        {
            usersWithSubscriptionsQuery = usersWithSubscriptionsQuery.Where(us =>
                us.User.DisplayName != null && us.User.DisplayName.Contains(query.UserDisplayName));
        }

        // دریافت تمام اشتراک‌های فیلتر شده
        var allSubscriptions = await usersWithSubscriptionsQuery
            .Select(us => new UserSubscriptionDetailDto
            {
                Id = us.Id,
                UserId = us.UserId,
                UserDisplayName = us.User.DisplayName ?? "بدون نام",
                UserPhoneNumber = us.User.PhoneNumber,
                PlanId = us.PlanId,
                PlanCode = us.Plan.Code,
                PlanTitle = us.Plan.Title,
                DurationMonths = us.Plan.DurationMonths,
                PlanPriceRial = us.Plan.PriceRial,
                StatusCode = us.Status.Code,
                StatusTitle = us.Status.Title,
                StartDate = us.StartDate,
                EndDate = us.EndDate,
                UnlimitedContactViews = us.Plan.UnlimitedContactViews,
                PaymentTransactionId = us.PaymentTransactionId,
                PurchasedAt = us.PurchasedAt,
                CancelledAt = us.CancelledAt,
                CreatedAt = us.CreatedAt
            })
            .ToListAsync();

        // گروه‌بندی در حافظه
        var usersGrouped = allSubscriptions
            .GroupBy(s => new
            {
                s.UserId,
                s.UserPhoneNumber,
                s.UserDisplayName
            })
            .Select(g => new UserWithSubscriptionsSummaryDto
            {
                UserId = g.Key.UserId,
                UserPhoneNumber = g.Key.UserPhoneNumber,
                UserDisplayName = g.Key.UserDisplayName,
                TotalSubscriptionsCount = g.Count(),
                ActiveSubscriptionsCount = g.Count(s => s.StatusCode == "Active"),
                ExpiredSubscriptionsCount = g.Count(s => s.StatusCode == "Expired"),
                Subscriptions = g.OrderByDescending(s => s.PurchasedAt).ToList()
            })
            .ToList();

        var totalCount = usersGrouped.Count;

        // Pagination
        var pagedUsers = usersGrouped
            .OrderByDescending(u => u.Subscriptions.Any() ? u.Subscriptions.Max(s => s.PurchasedAt) : DateTime.MinValue)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<UserWithSubscriptionsSummaryDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = pagedUsers
        };
    }

    public async Task<UserSubscriptionDetailDto> UpdateSubscriptionAsync(Guid subscriptionId, UpdateUserSubscriptionDto dto)
    {
        var subscription = await _db.UserSubscriptions
            .Include(us => us.User)
            .Include(us => us.Plan)
            .Include(us => us.Status)
            .FirstOrDefaultAsync(us => us.Id == subscriptionId);

        if (subscription == null)
            throw new NotFoundException("اشتراک مورد نظر یافت نشد.");

        // بررسی اینکه آیا status به "Cancelled" تغییر می‌کند
        bool isBeingCancelled = false;
        bool wasAlreadyCancelled = subscription.CancelledAt.HasValue;

        if (!string.IsNullOrWhiteSpace(dto.StatusCode) && dto.StatusCode == "Cancelled" && !wasAlreadyCancelled)
        {
            isBeingCancelled = true;
        }

        // به‌روزرسانی فیلدها
        if (dto.StartDate.HasValue)
            subscription.StartDate = dto.StartDate.Value;

        if (dto.EndDate.HasValue)
            subscription.EndDate = dto.EndDate.Value;

        if (!string.IsNullOrWhiteSpace(dto.StatusCode))
        {
            var status = await _db.SubscriptionStatuses
                .FirstOrDefaultAsync(s => s.Code == dto.StatusCode);
            
            if (status == null)
                throw new NotFoundException($"وضعیت با کد '{dto.StatusCode}' یافت نشد.");
            
            subscription.StatusId = status.Id;
        }

        if (dto.CancelledAt.HasValue)
            subscription.CancelledAt = dto.CancelledAt.Value;
        else if (dto.StatusCode != "Cancelled")
            subscription.CancelledAt = null;

        subscription.UpdatedAt = DateTime.Now;

        // اگر اشتراک لغو می‌شود، محاسبه و برگشت پول
        if (isBeingCancelled)
        {
            await ProcessSubscriptionCancellationAsync(subscription);
        }

        await _db.SaveChangesAsync();

        // بازگشت DTO به‌روز شده
        return new UserSubscriptionDetailDto
        {
            Id = subscription.Id,
            UserId = subscription.UserId,
            UserDisplayName = subscription.User.DisplayName ?? "بدون نام",
            UserPhoneNumber = subscription.User.PhoneNumber,
            PlanId = subscription.PlanId,
            PlanCode = subscription.Plan.Code,
            PlanTitle = subscription.Plan.Title,
            DurationMonths = subscription.Plan.DurationMonths,
            PlanPriceRial = subscription.Plan.PriceRial,
            StatusCode = subscription.Status.Code,
            StatusTitle = subscription.Status.Title,
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            UnlimitedContactViews = subscription.Plan.UnlimitedContactViews,
            PaymentTransactionId = subscription.PaymentTransactionId,
            PurchasedAt = subscription.PurchasedAt,
            CancelledAt = subscription.CancelledAt,
            CreatedAt = subscription.CreatedAt
        };
    }

    private async Task ProcessSubscriptionCancellationAsync(UserSubscription subscription)
    {
        var now = DateTime.Now;
        var originalAmountRial = subscription.Plan.PriceRial;

        // محاسبه تعداد روزهای استفاده شده و کل روزها
        int usedDays = 0;
        int totalDays = 0;
        long usedAmountRial = 0;
        long remainingAmountRial = 0;

        // تبدیل به تاریخ بدون ساعت برای مقایسه
        var nowDate = now.Date;
        var startDateOnly = subscription.StartDate.Date;
        var endDateOnly = subscription.EndDate.Date;

        // محاسبه تعداد کل روزها بر اساس DurationMonths (1 ماه = 30 روز)
        // اگر DurationMonths = 1 باشد، باید 30 روز باشد
        totalDays = subscription.Plan.DurationMonths * 30;

        // اگر تاریخ شروع بعد از امروز باشد (هنوز شروع نشده)
        if (startDateOnly > nowDate)
        {
            usedDays = 0;
            usedAmountRial = 0;
            remainingAmountRial = originalAmountRial;
        }
        // اگر تاریخ شروع امروز یا قبل از امروز باشد (شروع شده)
        else if (startDateOnly <= nowDate && subscription.EndDate > now)
        {
            // محاسبه روزهای استفاده شده (از تاریخ شروع تا امروز، شامل هر دو)
            // اگر startDate = 10/11 و now = 10/11 باشد، باید 1 روز باشد
            usedDays = (nowDate - startDateOnly).Days + 1;
            
            if (totalDays > 0)
            {
                var calculatedUsed = (decimal)originalAmountRial * usedDays / totalDays;
                usedAmountRial = RoundTo100Rial(calculatedUsed);
            }
            else
            {
                usedAmountRial = originalAmountRial;
            }
            
            remainingAmountRial = RoundTo100Rial(originalAmountRial - usedAmountRial);
        }
        // اگر اشتراک تمام شده، هیچ مبلغی برگشت داده نمی‌شود
        else
        {
            remainingAmountRial = 0;
        }

        // دریافت کارمزد خدمات از تنظیمات
        var serviceFeeSetting = await _db.SystemSettings
            .FirstOrDefaultAsync(s => s.Key == "SubscriptionCancellationServiceFeePercentage");

        decimal serviceFeePercentage = 10m;
        if (serviceFeeSetting != null && decimal.TryParse(serviceFeeSetting.Value, out var parsedFee))
        {
            serviceFeePercentage = parsedFee;
        }

        // محاسبه کارمزد خدمات از مبلغ باقیمانده
        var calculatedServiceFee = remainingAmountRial * serviceFeePercentage / 100m;
        long serviceFeeAmountRial = RoundTo100Rial(calculatedServiceFee);

        // محاسبه مبلغ نهایی برگشتی
        long refundAmountRial = RoundTo100Rial(remainingAmountRial - serviceFeeAmountRial);

        if (refundAmountRial <= 0)
        {
            refundAmountRial = 0;
        }

        // برگشت پول به کیف پول کاربر (اگر مبلغ برگشتی بیشتر از صفر باشد)
        if (refundAmountRial > 0)
        {
            var operationType = await _db.FinancialOperationTypes
                .FirstAsync(ot => ot.Code == "SubscriptionRefund");

            var referenceType = await _db.FinancialReferenceTypes
                .FirstAsync(rt => rt.Code == "Subscription");

            var calculationDescription = BuildCalculationDescription(
                originalAmountRial,
                usedDays,
                totalDays,
                usedAmountRial,
                remainingAmountRial,
                serviceFeePercentage,
                serviceFeeAmountRial,
                refundAmountRial,
                subscription.Plan.Title);

            var idempotencyKey = $"subscription_cancel_{subscription.Id}_{now:yyyyMMddHHmmss}";
            var creditResult = await _walletService.CreditAsync(
                subscription.UserId,
                refundAmountRial,
                operationType.Id,
                referenceType.Id,
                subscription.Id,
                idempotencyKey,
                calculationDescription);

            if (!creditResult.Success)
            {
                throw new AppException(creditResult.ErrorMessage ?? "خطا در واریز مبلغ به کیف پول.");
            }
        }

        // برگشت پورسانت به visitor (اگر پورسانتی پرداخت شده باشد)
        await ReverseSubscriptionCommissionAsync(subscription.Id);
    }

    private async Task ReverseSubscriptionCommissionAsync(Guid subscriptionId)
    {
        // پیدا کردن پورسانت‌های مربوط به این اشتراک
        var commissionTransactions = await _db.CommissionTransactions
            .Where(ct => 
                ct.ReferenceId == subscriptionId &&
                ct.ReferenceType == "UserSubscription" &&
                ct.CommissionType == "SubscriptionCommission")
            .ToListAsync();

        if (commissionTransactions.Count == 0)
        {
            return; // پورسانتی پرداخت نشده
        }

        // دریافت OperationType و ReferenceType برای Debit
        var operationType = await _db.FinancialOperationTypes
            .FirstAsync(ot => ot.Code == "CommissionReversal");

        var referenceType = await _db.FinancialReferenceTypes
            .FirstAsync(rt => rt.Code == "WalletTransaction");

        var now = DateTime.Now;

        foreach (var commission in commissionTransactions)
        {
            // بررسی اینکه آیا قبلاً برگشت داده شده است
            var existingReversal = await _db.WalletTransactions
                .FirstOrDefaultAsync(wt => 
                    wt.ReferenceId == commission.Id &&
                    wt.Description != null &&
                    wt.Description.Contains("برگشت پورسانت اشتراک لغو شده"));

            if (existingReversal != null)
            {
                continue; // قبلاً برگشت داده شده
            }

            // کسر مبلغ پورسانت از کیف پول visitor (حتی اگر موجودی منفی شود)
            var idempotencyKey = $"commission_reversal_{commission.Id}_{now:yyyyMMddHHmmss}";
            var debitResult = await _walletService.DebitAllowNegativeAsync(
                commission.VisitorUserId,
                commission.CommissionAmountRial,
                operationType.Id,
                referenceType.Id,
                commission.Id,
                idempotencyKey,
                $"برگشت پورسانت اشتراک لغو شده (مبلغ اصلی: {commission.OriginalAmountRial / 10m} تومان، پورسانت: {commission.CommissionAmountRial / 10m} تومان)");

            if (!debitResult.Success)
            {
                // Log error but continue with other commissions
                Console.WriteLine($"[UserSubscriptionManagement] Failed to reverse commission {commission.Id}: {debitResult.ErrorMessage}");
            }
        }
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

    /// <summary>
    /// گرد کردن به 100 ریال (گرد به پایین)
    /// </summary>
    private static long RoundTo100Rial(decimal amount)
    {
        return (long)(Math.Floor(amount / 100m) * 100m);
    }
}


















