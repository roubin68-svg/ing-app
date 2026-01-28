using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Interfaces.Financial;
using IngApp.Application.Features.Financial.DTO;
using IngApp.Domain.Entities.Offers;
using IngApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IngApp.Infrastructure.Services.Financial;

public class UnlockContactService : IUnlockContactService
{
    private readonly AppDbContext _db;
    private readonly IWalletService _walletService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly ICommissionService _commissionService;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public UnlockContactService(
        AppDbContext db,
        IWalletService walletService,
        ISubscriptionService subscriptionService,
        ICommissionService commissionService,
        IServiceScopeFactory serviceScopeFactory)
    {
        _db = db;
        _walletService = walletService;
        _subscriptionService = subscriptionService;
        _commissionService = commissionService;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<bool> IsUnlockedAsync(int offerId, Guid userId)
    {
        return await _db.OfferContactUnlocks
            .AnyAsync(u => u.OfferId == offerId && u.UserId == userId);
    }

    public async Task<UnlockContactResultDto> UnlockContactAsync(int offerId, Guid userId, string idempotencyKey)
    {
        // بررسی اینکه آیا قبلاً Unlock شده است
        var existingUnlock = await _db.OfferContactUnlocks
            .FirstOrDefaultAsync(u => u.OfferId == offerId && u.UserId == userId);

        if (existingUnlock != null)
        {
            return new UnlockContactResultDto
            {
                IsUnlocked = true,
                Charged = false,
                TransactionId = existingUnlock.ChargedTransactionId
            };
        }

        // بررسی اشتراک فعال با UnlimitedContactViews
        var hasUnlimitedContact = await _subscriptionService.HasUnlimitedContactViewsAsync(userId);
        
        if (hasUnlimitedContact)
        {
            // دریافت UnlockSourceType (Subscription)
            var subscriptionSourceType = await _db.UnlockSourceTypes
                .FirstOrDefaultAsync(st => st.Code == "Subscription");

            if (subscriptionSourceType == null)
            {
                throw new AppException("نوع منبع 'Subscription' یافت نشد. لطفاً با مدیر سیستم تماس بگیرید.");
            }

            // ثبت Unlock بدون هزینه
            var subscriptionUnlock = new OfferContactUnlock
            {
                Id = Guid.NewGuid(),
                OfferId = offerId,
                UserId = userId,
                UnlockedAt = DateTime.UtcNow,
                ChargedTransactionId = null, // بدون تراکنش مالی
                SourceTypeId = subscriptionSourceType.Id
            };

            _db.OfferContactUnlocks.Add(subscriptionUnlock);
            await _db.SaveChangesAsync();

            return new UnlockContactResultDto
            {
                IsUnlocked = true,
                Charged = false,
                TransactionId = null
            };
        }

        // دریافت تعرفه UnlockContactFee
        var pricing = await _db.Pricings
            .FirstOrDefaultAsync(p => p.Code == "UnlockContactFee" && p.IsActive);

        if (pricing == null)
        {
            throw new AppException("تعرفه باز کردن اطلاعات تماس یافت نشد. لطفاً با مدیر سیستم تماس بگیرید.");
        }

        // دریافت OperationType و ReferenceType
        var operationType = await _db.FinancialOperationTypes
            .FirstOrDefaultAsync(ot => ot.Code == "UnlockContactFee");

        if (operationType == null)
        {
            throw new AppException("نوع عملیات 'UnlockContactFee' یافت نشد. لطفاً با مدیر سیستم تماس بگیرید.");
        }

        var referenceType = await _db.FinancialReferenceTypes
            .FirstOrDefaultAsync(rt => rt.Code == "Offer");

        if (referenceType == null)
        {
            throw new AppException("نوع مرجع 'Offer' یافت نشد. لطفاً با مدیر سیستم تماس بگیرید.");
        }

        // دریافت UnlockSourceType (Paid)
        var sourceType = await _db.UnlockSourceTypes
            .FirstOrDefaultAsync(st => st.Code == "Paid");

        if (sourceType == null)
        {
            throw new AppException("نوع منبع 'Paid' یافت نشد. لطفاً با مدیر سیستم تماس بگیرید.");
        }

        // Debit از Wallet
        // ReferenceId باید Guid باشد، اما OfferId یک int است
        // برای تبدیل int به Guid، از یک روش استاندارد استفاده می‌کنیم
        // یا می‌توانیم null بگذاریم و فقط در Description ذخیره کنیم
        var debitResult = await _walletService.DebitAsync(
            userId,
            pricing.AmountRial,
            operationType.Id,
            referenceType.Id,
            null, // ReferenceId برای Offer می‌تواند null باشد یا از Description استفاده کنیم
            idempotencyKey,
            $"هزینه باز کردن اطلاعات تماس آگهی #{offerId}");

        if (!debitResult.Success)
        {
            return new UnlockContactResultDto
            {
                IsUnlocked = false,
                Charged = false,
                ErrorMessage = debitResult.ErrorMessage ?? "خطا در پردازش تراکنش"
            };
        }

        // ثبت Unlock
        var unlock = new OfferContactUnlock
        {
            Id = Guid.NewGuid(),
            OfferId = offerId,
            UserId = userId,
            UnlockedAt = DateTime.UtcNow,
            ChargedTransactionId = debitResult.TransactionId,
            SourceTypeId = sourceType.Id
        };

        _db.OfferContactUnlocks.Add(unlock);
        await _db.SaveChangesAsync();

        // پرداخت پورسانت (اگر خریدار از طریق بازاریاب معرفی شده باشد)
        // فقط در صورتی که واقعاً هزینه پرداخت شده باشد (Charged = true)
        // استفاده از ServiceScopeFactory برای ایجاد DbContext جدید در background task
        if (debitResult.TransactionId != Guid.Empty)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    // ایجاد scope جدید برای background task تا DbContext جداگانه داشته باشد
                    using var scope = _serviceScopeFactory.CreateScope();
                    var commissionService = scope.ServiceProvider.GetRequiredService<ICommissionService>();
                    
                    var commissionResult = await commissionService.ProcessUnlockContactCommissionAsync(
                        userId,
                        unlock.Id,
                        pricing.AmountRial);
                    
                    if (commissionResult != null && !commissionResult.Success)
                    {
                        Console.WriteLine($"[Commission] Failed to process unlock contact commission: {commissionResult.ErrorMessage}");
                    }
                    else if (commissionResult == null)
                    {
                        Console.WriteLine($"[Commission] No commission processed for unlock {unlock.Id} - Buyer may not have a referrer");
                    }
                    else
                    {
                        Console.WriteLine($"[Commission] Successfully processed unlock contact commission: {commissionResult.CommissionAmountRial / 10m} Toman");
                    }
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the unlock operation
                    Console.WriteLine($"[Commission] Error processing unlock contact commission: {ex.Message}");
                    Console.WriteLine($"[Commission] Stack trace: {ex.StackTrace}");
                }
            });
        }

        return new UnlockContactResultDto
        {
            IsUnlocked = true,
            Charged = true,
            ChargedAmountRial = pricing.AmountRial,
            TransactionId = debitResult.TransactionId
        };
    }
}


