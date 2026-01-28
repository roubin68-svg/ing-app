using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Interfaces.Financial;
using IngApp.Application.Common.Interfaces.Users;
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Users.DTO;
using IngApp.Domain.Entities.Financial;
using IngApp.Domain.Entities.Users;
using IngApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IngApp.Infrastructure.Services.Users;

public class VisitorManagementService : IVisitorManagementService
{
    private readonly AppDbContext _db;
    private readonly IVisitorProfileService _visitorProfileService;

    public VisitorManagementService(AppDbContext db, IVisitorProfileService visitorProfileService)
    {
        _db = db;
        _visitorProfileService = visitorProfileService;
    }

    public async Task<PagedResult<VisitorManagementDto>> GetPagedAsync(VisitorListQueryDto filter)
    {
        var query = _db.VisitorProfiles
            .Include(vp => vp.User)
            .AsQueryable();

        // فیلتر جستجو
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            query = query.Where(vp =>
                vp.User.PhoneNumber.Contains(search) ||
                (vp.User.DisplayName != null && vp.User.DisplayName.Contains(search)) ||
                vp.ReferralCode.Contains(search) ||
                (vp.BusinessName != null && vp.BusinessName.Contains(search)));
        }

        // فیلتر IsActive
        if (filter.IsActive.HasValue)
        {
            query = query.Where(vp => vp.IsActive == filter.IsActive.Value);
        }

        // مرتب‌سازی
        query = (filter.SortBy?.ToLower(), filter.SortDesc) switch
        {
            ("referralcode", true) => query.OrderByDescending(vp => vp.ReferralCode),
            ("referralcode", false) => query.OrderBy(vp => vp.ReferralCode),
            ("buyercount", true) => query.OrderByDescending(vp => _db.BuyerProfiles.Count(bp => bp.ReferredByVisitorId == vp.Id)),
            ("buyercount", false) => query.OrderBy(vp => _db.BuyerProfiles.Count(bp => bp.ReferredByVisitorId == vp.Id)),
            ("totalcommission", true) => query.OrderByDescending(vp => 
                _db.CommissionTransactions
                    .Where(ct => ct.VisitorUserId == vp.UserId)
                    .Sum(ct => (long?)ct.CommissionAmountRial) ?? 0),
            ("totalcommission", false) => query.OrderBy(vp => 
                _db.CommissionTransactions
                    .Where(ct => ct.VisitorUserId == vp.UserId)
                    .Sum(ct => (long?)ct.CommissionAmountRial) ?? 0),
            ("createdat", false) => query.OrderBy(vp => vp.CreatedAt),
            _ => query.OrderByDescending(vp => vp.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var page = filter.Page <= 0 ? 1 : filter.Page;
        var pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

        var visitors = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtoList = new List<VisitorManagementDto>();
        foreach (var visitor in visitors)
        {
            var buyerCount = await _db.BuyerProfiles
                .CountAsync(bp => bp.ReferredByVisitorId == visitor.Id);

            var totalCommission = await _db.CommissionTransactions
                .Where(ct => ct.VisitorUserId == visitor.UserId)
                .SumAsync(ct => (long?)ct.CommissionAmountRial) ?? 0;

            dtoList.Add(new VisitorManagementDto
            {
                Id = visitor.Id,
                UserId = visitor.UserId,
                UserPhoneNumber = visitor.User.PhoneNumber,
                UserDisplayName = visitor.User.DisplayName,
                ReferralCode = visitor.ReferralCode,
                BusinessName = visitor.BusinessName,
                ContactMobile = visitor.ContactMobile,
                Province = visitor.Province,
                City = visitor.City,
                Address = visitor.Address,
                Description = visitor.Description,
                IsActive = visitor.IsActive,
                BuyerCount = buyerCount,
                TotalCommissionRial = totalCommission,
                CreatedAt = visitor.CreatedAt,
                UpdatedAt = visitor.UpdatedAt
            });
        }

        return new PagedResult<VisitorManagementDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = dtoList
        };
    }

    public async Task<VisitorManagementDto?> GetByIdAsync(Guid visitorProfileId)
    {
        var visitor = await _db.VisitorProfiles
            .Include(vp => vp.User)
            .FirstOrDefaultAsync(vp => vp.Id == visitorProfileId);

        if (visitor == null)
            return null;

        var buyerCount = await _db.BuyerProfiles
            .CountAsync(bp => bp.ReferredByVisitorId == visitor.Id);

        var totalCommission = await _db.CommissionTransactions
            .Where(ct => ct.VisitorUserId == visitor.UserId)
            .SumAsync(ct => (long?)ct.CommissionAmountRial) ?? 0;

        return new VisitorManagementDto
        {
            Id = visitor.Id,
            UserId = visitor.UserId,
            UserPhoneNumber = visitor.User.PhoneNumber,
            UserDisplayName = visitor.User.DisplayName,
            ReferralCode = visitor.ReferralCode,
            BusinessName = visitor.BusinessName,
            ContactMobile = visitor.ContactMobile,
            Province = visitor.Province,
            City = visitor.City,
            Address = visitor.Address,
            Description = visitor.Description,
            IsActive = visitor.IsActive,
            BuyerCount = buyerCount,
            TotalCommissionRial = totalCommission,
            CreatedAt = visitor.CreatedAt,
            UpdatedAt = visitor.UpdatedAt
        };
    }

    public async Task<VisitorManagementDto> CreateAsync(CreateVisitorDto dto)
    {
        // بررسی اینکه User وجود دارد
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == dto.UserId);

        if (user == null)
            throw new NotFoundException("کاربر یافت نشد.");

        // بررسی اینکه قبلاً VisitorProfile ندارد
        var existingProfile = await _db.VisitorProfiles
            .FirstOrDefaultAsync(vp => vp.UserId == dto.UserId);

        if (existingProfile != null)
            throw new ValidationException(new() { "این کاربر قبلاً بازاریاب است و نمی‌تواند دوباره اضافه شود." });

        // تولید ReferralCode یکتا
        var referralCode = await GenerateUniqueReferralCodeAsync();

        var visitorProfile = new VisitorProfile
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            ReferralCode = referralCode,
            BusinessName = dto.BusinessName,
            ContactMobile = dto.ContactMobile,
            Province = dto.Province,
            City = dto.City,
            Address = dto.Address,
            Description = dto.Description,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.VisitorProfiles.Add(visitorProfile);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(visitorProfile.Id) 
            ?? throw new AppException("خطا در ایجاد VisitorProfile");
    }

    public async Task<VisitorManagementDto> UpdateAsync(Guid visitorProfileId, UpdateVisitorDto dto)
    {
        var visitor = await _db.VisitorProfiles
            .FirstOrDefaultAsync(vp => vp.Id == visitorProfileId);

        if (visitor == null)
            throw new NotFoundException("Visitor یافت نشد.");

        visitor.BusinessName = dto.BusinessName;
        visitor.ContactMobile = dto.ContactMobile;
        visitor.Province = dto.Province;
        visitor.City = dto.City;
        visitor.Address = dto.Address;
        visitor.Description = dto.Description;
        visitor.IsActive = dto.IsActive;
        visitor.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return await GetByIdAsync(visitorProfileId) 
            ?? throw new AppException("خطا در به‌روزرسانی VisitorProfile");
    }

    public async Task ChangeStatusAsync(Guid visitorProfileId, bool isActive)
    {
        var visitor = await _db.VisitorProfiles
            .FirstOrDefaultAsync(vp => vp.Id == visitorProfileId);

        if (visitor == null)
            throw new NotFoundException("Visitor یافت نشد.");

        visitor.IsActive = isActive;
        visitor.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid visitorProfileId)
    {
        var visitor = await _db.VisitorProfiles
            .FirstOrDefaultAsync(vp => vp.Id == visitorProfileId);

        if (visitor == null)
            throw new NotFoundException("Visitor یافت نشد.");

        // بررسی اینکه آیا Buyer دارد یا نه
        var hasBuyers = await _db.BuyerProfiles
            .AnyAsync(bp => bp.ReferredByVisitorId == visitorProfileId);

        if (hasBuyers)
            throw new ValidationException(new() { "نمی‌توان Visitor را حذف کرد زیرا Buyer هایی دارد." });

        _db.VisitorProfiles.Remove(visitor);
        await _db.SaveChangesAsync();
    }

    public async Task<List<BuyerForVisitorDto>> GetBuyersAsync(Guid visitorProfileId)
    {
        var buyers = await _db.BuyerProfiles
            .Include(bp => bp.User)
            .Where(bp => bp.ReferredByVisitorId == visitorProfileId)
            .OrderByDescending(bp => bp.CreatedAt)
            .ToListAsync();

        return buyers.Select(bp => new BuyerForVisitorDto
        {
            BuyerProfileId = bp.Id,
            UserId = bp.UserId,
            UserPhoneNumber = bp.User.PhoneNumber,
            UserDisplayName = bp.User.DisplayName,
            BusinessName = bp.BusinessName,
            ReferredAt = bp.CreatedAt
        }).ToList();
    }

    public async Task<BuyerForVisitorDto> AddBuyerAsync(Guid visitorProfileId, AddBuyerToVisitorDto dto)
    {
        // بررسی Visitor
        var visitor = await _db.VisitorProfiles
            .Include(vp => vp.User)
            .FirstOrDefaultAsync(vp => vp.Id == visitorProfileId);

        if (visitor == null)
            throw new NotFoundException("Visitor یافت نشد.");

        if (!visitor.IsActive)
            throw new ValidationException(new() { "Visitor غیرفعال است." });

        var mobile = dto.Mobile.Trim();

        // بررسی اینکه Visitor خودش را به عنوان Buyer اضافه نکرده باشد
        if (visitor.User.PhoneNumber == mobile)
            throw new ValidationException(new() { "یک بازاریاب نمی‌تواند خودش را به عنوان خریدار خودش اضافه کند." });

        // بررسی اینکه User با این Mobile وجود دارد یا نه
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == mobile);

        BuyerProfile buyerProfile;

        if (user == null)
        {
            // User وجود ندارد - باید ایجاد شود
            // دریافت UserType برای Buyer
            var buyerUserType = await _db.UserTypes
                .FirstOrDefaultAsync(ut => ut.Code == "Buyer" && ut.IsActive);

            if (buyerUserType == null)
                throw new AppException("نوع کاربر Buyer در سیستم یافت نشد.");

            // ایجاد User
            user = new User
            {
                PhoneNumber = mobile,
                DisplayName = dto.BuyerName?.Trim(),
                UserTypeId = buyerUserType.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // ایجاد BuyerProfile
            buyerProfile = new BuyerProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                BusinessName = dto.BuyerName?.Trim(),
                ReferredByVisitorId = visitorProfileId,
                CreatedAt = DateTime.UtcNow
            };

            _db.BuyerProfiles.Add(buyerProfile);
        }
        else
        {
            // User وجود دارد - بررسی BuyerProfile
            buyerProfile = await _db.BuyerProfiles
                .FirstOrDefaultAsync(bp => bp.UserId == user.Id);

            if (buyerProfile == null)
            {
                // BuyerProfile وجود ندارد - ایجاد می‌کنیم
                buyerProfile = new BuyerProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    ReferredByVisitorId = visitorProfileId,
                    CreatedAt = DateTime.UtcNow
                };

                _db.BuyerProfiles.Add(buyerProfile);
            }
            else
            {
                // BuyerProfile وجود دارد - بررسی اینکه آیا Visitor دیگری دارد
                if (buyerProfile.ReferredByVisitorId != null && buyerProfile.ReferredByVisitorId != visitorProfileId)
                {
                    throw new ValidationException(new() { "این Buyer قبلاً توسط Visitor دیگری معرفی شده است." });
                }

                // اگر ReferredByVisitorId null است، تنظیم می‌کنیم
                if (buyerProfile.ReferredByVisitorId == null)
                {
                    buyerProfile.ReferredByVisitorId = visitorProfileId;
                    buyerProfile.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        await _db.SaveChangesAsync();

        // بارگذاری مجدد با Include
        buyerProfile = await _db.BuyerProfiles
            .Include(bp => bp.User)
            .FirstOrDefaultAsync(bp => bp.Id == buyerProfile.Id);

        if (buyerProfile == null)
            throw new AppException("خطا در ایجاد BuyerProfile");

        return new BuyerForVisitorDto
        {
            BuyerProfileId = buyerProfile.Id,
            UserId = buyerProfile.UserId,
            UserPhoneNumber = buyerProfile.User.PhoneNumber,
            UserDisplayName = buyerProfile.User.DisplayName,
            BusinessName = buyerProfile.BusinessName,
            ReferredAt = buyerProfile.CreatedAt
        };
    }

    public async Task RemoveBuyerAsync(Guid visitorProfileId, Guid buyerProfileId)
    {
        var buyerProfile = await _db.BuyerProfiles
            .FirstOrDefaultAsync(bp => bp.Id == buyerProfileId && bp.ReferredByVisitorId == visitorProfileId);

        if (buyerProfile == null)
            throw new NotFoundException("Buyer یافت نشد یا به این Visitor تعلق ندارد.");

        // حذف ReferralCode (تنظیم ReferredByVisitorId به null)
        buyerProfile.ReferredByVisitorId = null;
        buyerProfile.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task<List<VisitorCommissionRuleDto>> GetCommissionRulesAsync(Guid visitorProfileId)
    {
        // دریافت تمام CommissionRule های فعال
        var defaultRules = await _db.CommissionRules
            .Where(cr => cr.IsActive)
            .ToListAsync();

        // دریافت VisitorCommissionRule های این Visitor
        var visitorRules = await _db.VisitorCommissionRules
            .Where(vcr => vcr.VisitorProfileId == visitorProfileId)
            .ToListAsync();

        var result = new List<VisitorCommissionRuleDto>();

        foreach (var defaultRule in defaultRules)
        {
            var visitorRule = visitorRules.FirstOrDefault(vr => vr.CommissionRuleCode == defaultRule.Code);

            result.Add(new VisitorCommissionRuleDto
            {
                Id = visitorRule?.Id ?? 0,
                CommissionRuleCode = defaultRule.Code,
                CommissionRuleTitle = defaultRule.Title,
                CommissionPercentage = visitorRule?.CommissionPercentage,
                DefaultCommissionPercentage = defaultRule.CommissionPercentage,
                IsActive = visitorRule?.IsActive ?? true,
                EffectiveFrom = visitorRule?.EffectiveFrom,
                EffectiveTo = visitorRule?.EffectiveTo
            });
        }

        return result;
    }

    public async Task<VisitorCommissionRuleDto> SetCommissionRuleAsync(Guid visitorProfileId, SetVisitorCommissionRuleDto dto)
    {
        // بررسی Visitor
        var visitor = await _db.VisitorProfiles
            .FirstOrDefaultAsync(vp => vp.Id == visitorProfileId);
        if (visitor == null)
            throw new NotFoundException("Visitor یافت نشد.");

        // بررسی CommissionRule
        var defaultRule = await _db.CommissionRules
            .FirstOrDefaultAsync(cr => cr.Code == dto.CommissionRuleCode && cr.IsActive);
        if (defaultRule == null)
            throw new NotFoundException($"قانون پورسانت '{dto.CommissionRuleCode}' یافت نشد.");

        // بررسی VisitorCommissionRule موجود (ممکن است نسخه‌های قدیمی برای تاریخچه باشد)
        var existingRule = await _db.VisitorCommissionRules
            .FirstOrDefaultAsync(vcr =>
                vcr.VisitorProfileId == visitorProfileId &&
                vcr.CommissionRuleCode == dto.CommissionRuleCode &&
                vcr.IsActive);

        // جلوگیری از هم‌پوشانی بازه‌های زمانی برای این Visitor + CommissionRuleCode
        // فقط قوانین فعال دیگر را در نظر می‌گیریم
        var overlapQuery = _db.VisitorCommissionRules.Where(vcr =>
            vcr.VisitorProfileId == visitorProfileId &&
            vcr.CommissionRuleCode == dto.CommissionRuleCode &&
            vcr.IsActive);

        if (existingRule != null)
        {
            // در صورت آپدیت، همین رکورد را از بررسی هم‌پوشانی خارج می‌کنیم
            overlapQuery = overlapQuery.Where(vcr => vcr.Id != existingRule.Id);
        }

        if (dto.EffectiveFrom.HasValue && dto.EffectiveTo.HasValue && dto.EffectiveFrom.Value > dto.EffectiveTo.Value)
        {
            throw new ValidationException(new() { "تاریخ شروع باید قبل از تاریخ پایان باشد." });
        }

        var hasOverlap = await overlapQuery.AnyAsync(vcr =>
            (vcr.EffectiveFrom == null || dto.EffectiveTo == null || vcr.EffectiveFrom <= dto.EffectiveTo) &&
            (dto.EffectiveFrom == null || vcr.EffectiveTo == null || vcr.EffectiveTo >= dto.EffectiveFrom));

        if (hasOverlap)
        {
            throw new ValidationException(new()
            {
                "برای این بازاریاب و این نوع پورسانت، در بازه‌ی زمانی انتخاب‌شده قبلاً قانون دیگری ثبت شده است."
            });
        }

        if (existingRule != null)
        {
            // به‌روزرسانی قانون موجود (نسخه فعلی)
            existingRule.CommissionPercentage = dto.CommissionPercentage;
            existingRule.IsActive = dto.IsActive;
            existingRule.EffectiveFrom = dto.EffectiveFrom;
            existingRule.EffectiveTo = dto.EffectiveTo;
            existingRule.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            // ایجاد جدید
            existingRule = new VisitorCommissionRule
            {
                VisitorProfileId = visitorProfileId,
                CommissionRuleCode = dto.CommissionRuleCode,
                CommissionPercentage = dto.CommissionPercentage,
                IsActive = dto.IsActive,
                EffectiveFrom = dto.EffectiveFrom,
                EffectiveTo = dto.EffectiveTo,
                CreatedAt = DateTime.UtcNow
            };

            _db.VisitorCommissionRules.Add(existingRule);
        }

        await _db.SaveChangesAsync();

        return new VisitorCommissionRuleDto
        {
            Id = existingRule.Id,
            CommissionRuleCode = existingRule.CommissionRuleCode,
            CommissionRuleTitle = defaultRule.Title,
            CommissionPercentage = existingRule.CommissionPercentage,
            DefaultCommissionPercentage = defaultRule.CommissionPercentage,
            IsActive = existingRule.IsActive,
            EffectiveFrom = existingRule.EffectiveFrom,
            EffectiveTo = existingRule.EffectiveTo
        };
    }

    public async Task RemoveCommissionRuleAsync(Guid visitorProfileId, string commissionRuleCode)
    {
        // Soft Delete: تمامی قوانین فعال برای این Visitor + Code را غیرفعال می‌کنیم
        var activeRules = await _db.VisitorCommissionRules
            .Where(vcr =>
                vcr.VisitorProfileId == visitorProfileId &&
                vcr.CommissionRuleCode == commissionRuleCode &&
                vcr.IsActive)
            .ToListAsync();

        if (activeRules.Count == 0)
            return;

        var now = DateTime.UtcNow;
        foreach (var rule in activeRules)
        {
            rule.IsActive = false;

            if (!rule.EffectiveTo.HasValue || rule.EffectiveTo.Value > now)
            {
                rule.EffectiveTo = now;
            }

            rule.UpdatedAt = now;
        }

        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// تولید کد معرف یکتا (8 کاراکتر حروف و اعداد)
    /// </summary>
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









