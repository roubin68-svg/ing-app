using IngApp.Application.Common.Interfaces.Users;
using IngApp.Application.Features.Users.DTO;
using IngApp.Domain.Entities.Users;
using IngApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;

namespace IngApp.Infrastructure.Services.Users;

public class BuyerProfileService : IBuyerProfileService
{
    private readonly AppDbContext _db;
    private readonly IVisitorProfileService _visitorProfileService;

    public BuyerProfileService(AppDbContext db, IVisitorProfileService visitorProfileService)
    {
        _db = db;
        _visitorProfileService = visitorProfileService;
    }

    public async Task<BuyerProfileDto?> GetMyProfileAsync(Guid userId)
    {
        return await GetByUserIdAsync(userId);
    }

    public async Task<BuyerProfileDto?> GetByUserIdAsync(Guid userId)
    {
        try
        {
            var profile = await _db.BuyerProfiles
                .Include(bp => bp.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(bp => bp.UserId == userId);

            if (profile == null)
                return null;

            // اگر ReferredByVisitorId وجود دارد، VisitorProfile را جداگانه load می‌کنیم
            VisitorProfile? referredByVisitor = null;
            if (profile.ReferredByVisitorId.HasValue)
            {
                referredByVisitor = await _db.VisitorProfiles
                    .Include(v => v.User)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(v => v.Id == profile.ReferredByVisitorId.Value);
            }

            // محاسبه ReferredByVisitorName با بررسی null
            string? referredByVisitorName = null;
            string? referredByVisitorCode = null;
            if (referredByVisitor != null)
            {
                referredByVisitorCode = referredByVisitor.ReferralCode;
                referredByVisitorName = referredByVisitor.BusinessName;
                if (string.IsNullOrWhiteSpace(referredByVisitorName) && referredByVisitor.User != null)
                {
                    referredByVisitorName = referredByVisitor.User.DisplayName;
                }
            }

            return new BuyerProfileDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                UserPhoneNumber = profile.User.PhoneNumber,
                UserDisplayName = profile.User.DisplayName,
                BusinessName = profile.BusinessName,
                ContactMobile = profile.ContactMobile,
                Province = profile.Province,
                City = profile.City,
                Address = profile.Address,
                Description = profile.Description,
                ReferredByVisitorId = profile.ReferredByVisitorId,
                ReferredByVisitorCode = referredByVisitorCode,
                ReferredByVisitorName = referredByVisitorName,
                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt
            };
        }
        catch (Exception ex)
        {
            // Log the error for debugging
            Console.WriteLine($"[BuyerProfileService] Error in GetByUserIdAsync for userId {userId}: {ex.Message}");
            Console.WriteLine($"[BuyerProfileService] Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[BuyerProfileService] Inner exception: {ex.InnerException.Message}");
            }
            throw; // Re-throw to let the middleware handle it
        }
    }

    public async Task<BuyerProfileDto> UpsertMyProfileAsync(Guid userId, UpsertBuyerProfileDto dto)
    {
        var existingProfile = await _db.BuyerProfiles
            .FirstOrDefaultAsync(bp => bp.UserId == userId);

        Guid? referredByVisitorId = null;

        // اگر ReferrerVisitorCode ارائه شده باشد، Visitor را پیدا کن
        if (!string.IsNullOrWhiteSpace(dto.ReferrerVisitorCode))
        {
            var visitorProfile = await _visitorProfileService.GetByReferralCodeAsync(dto.ReferrerVisitorCode.Trim().ToUpper());
            
            if (visitorProfile == null)
            {
                throw new InvalidOperationException($"کد معرف '{dto.ReferrerVisitorCode}' یافت نشد.");
            }

            if (!visitorProfile.IsActive)
            {
                throw new InvalidOperationException($"کد معرف '{dto.ReferrerVisitorCode}' غیرفعال است.");
            }

            // بررسی اینکه Buyer خودش را به عنوان بازاریاب انتخاب نکرده باشد
            if (visitorProfile.UserId == userId)
                throw new InvalidOperationException("شما نمی‌توانید خودتان را به عنوان بازاریاب انتخاب کنید.");

            referredByVisitorId = visitorProfile.Id;
        }

        if (existingProfile == null)
        {
            // ایجاد پروفایل جدید
            existingProfile = new BuyerProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                BusinessName = dto.BusinessName,
                ContactMobile = dto.ContactMobile,
                Province = dto.Province,
                City = dto.City,
                Address = dto.Address,
                Description = dto.Description,
                ReferredByVisitorId = referredByVisitorId,
                CreatedAt = DateTime.UtcNow
            };

            _db.BuyerProfiles.Add(existingProfile);
        }
        else
        {
            // به‌روزرسانی پروفایل موجود
            existingProfile.BusinessName = dto.BusinessName;
            existingProfile.ContactMobile = dto.ContactMobile;
            existingProfile.Province = dto.Province;
            existingProfile.City = dto.City;
            existingProfile.Address = dto.Address;
            existingProfile.Description = dto.Description;
            
            // ReferredByVisitorId فقط یکبار قابل تنظیم است (قفل است)
            // اگر قبلاً تنظیم نشده باشد و ReferrerVisitorCode ارائه شده باشد، تنظیم می‌شود
            // اگر قبلاً تنظیم شده باشد، دیگر قابل تغییر نیست (مگر توسط Admin)
            if (existingProfile.ReferredByVisitorId == null && !string.IsNullOrWhiteSpace(dto.ReferrerVisitorCode))
            {
                existingProfile.ReferredByVisitorId = referredByVisitorId;
            }
            // اگر ReferredByVisitorId قبلاً تنظیم شده باشد، حتی اگر ReferrerVisitorCode ارائه شده باشد، تغییر نمی‌دهیم
            
            existingProfile.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        return await GetByUserIdAsync(userId) ?? throw new InvalidOperationException("خطا در ایجاد پروفایل Buyer");
    }
}

