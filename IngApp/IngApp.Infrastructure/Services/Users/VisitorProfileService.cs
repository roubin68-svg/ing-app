using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Interfaces.Users;
using IngApp.Application.Features.Users.DTO;
using IngApp.Domain.Entities.Users;
using IngApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IngApp.Infrastructure.Services.Users;

public class VisitorProfileService : IVisitorProfileService
{
    private readonly AppDbContext _db;

    public VisitorProfileService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<VisitorProfileDto?> GetMyProfileAsync(Guid userId)
    {
        return await GetByUserIdAsync(userId);
    }

    public async Task<VisitorProfileDto?> GetByUserIdAsync(Guid userId)
    {
        var profile = await _db.VisitorProfiles
            .Include(vp => vp.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(vp => vp.UserId == userId);

        if (profile == null)
            return null;

        return new VisitorProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            UserPhoneNumber = profile.User.PhoneNumber,
            UserDisplayName = profile.User.DisplayName,
            ReferralCode = profile.ReferralCode,
            BusinessName = profile.BusinessName,
            ContactMobile = profile.ContactMobile,
            Province = profile.Province,
            City = profile.City,
            Address = profile.Address,
            Description = profile.Description,
            IsActive = profile.IsActive,
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt
        };
    }

    public async Task<VisitorProfileDto?> GetByReferralCodeAsync(string referralCode)
    {
        var profile = await _db.VisitorProfiles
            .Include(vp => vp.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(vp => vp.ReferralCode == referralCode && vp.IsActive);

        if (profile == null)
            return null;

        return new VisitorProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            UserPhoneNumber = profile.User.PhoneNumber,
            UserDisplayName = profile.User.DisplayName,
            ReferralCode = profile.ReferralCode,
            BusinessName = profile.BusinessName,
            ContactMobile = profile.ContactMobile,
            Province = profile.Province,
            City = profile.City,
            Address = profile.Address,
            Description = profile.Description,
            IsActive = profile.IsActive,
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt
        };
    }

    public async Task<VisitorProfileDto> UpsertMyProfileAsync(Guid userId, UpsertVisitorProfileDto dto)
    {
        var existingProfile = await _db.VisitorProfiles
            .FirstOrDefaultAsync(vp => vp.UserId == userId);

        if (existingProfile == null)
        {
            // ایجاد پروفایل جدید
            var referralCode = await GenerateUniqueReferralCodeAsync();
            
            existingProfile = new VisitorProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ReferralCode = referralCode,
                BusinessName = dto.BusinessName,
                ContactMobile = dto.ContactMobile,
                Province = dto.Province,
                City = dto.City,
                Address = dto.Address,
                Description = dto.Description,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.Now
            };

            _db.VisitorProfiles.Add(existingProfile);
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
            existingProfile.IsActive = dto.IsActive;
            existingProfile.UpdatedAt = DateTime.Now;
        }

        await _db.SaveChangesAsync();

        return await GetByUserIdAsync(userId) ?? throw new InvalidOperationException("خطا در ایجاد پروفایل Visitor");
    }

    /// <summary>
    /// تولید کد معرف یکتا (4 کاراکتر: حروف و اعداد - ساده و قابل خواندن)
    /// </summary>
    private async Task<string> GenerateUniqueReferralCodeAsync()
    {
        // استفاده از حروف و اعداد ساده برای تلفظ راحت
        const string letters = "ABCDEFGHJKLMNPRSTUVWXYZ"; // حذف I, O, Q برای جلوگیری از اشتباه
        const string numbers = "0123456789";
        var random = new Random();
        string code;
        int maxAttempts = 1000; // جلوگیری از حلقه بی‌نهایت
        int attempts = 0;
        
        do
        {
            // الگوی ساده: حرف-عدد-حرف-عدد یا عدد-حرف-عدد-حرف
            if (random.Next(2) == 0)
            {
                // الگوی: حرف-عدد-حرف-عدد (مثال: A1B2)
                code = $"{letters[random.Next(letters.Length)]}" +
                       $"{numbers[random.Next(numbers.Length)]}" +
                       $"{letters[random.Next(letters.Length)]}" +
                       $"{numbers[random.Next(numbers.Length)]}";
            }
            else
            {
                // الگوی: عدد-حرف-عدد-حرف (مثال: 1A2B)
                code = $"{numbers[random.Next(numbers.Length)]}" +
                       $"{letters[random.Next(letters.Length)]}" +
                       $"{numbers[random.Next(numbers.Length)]}" +
                       $"{letters[random.Next(letters.Length)]}";
            }
            
            attempts++;
            if (attempts >= maxAttempts)
            {
                throw new AppException("امکان تولید کد معرف یکتا وجود ندارد. لطفاً دوباره تلاش کنید.");
            }
        } while (await _db.VisitorProfiles.AnyAsync(vp => vp.ReferralCode == code));

        return code;
    }
}

