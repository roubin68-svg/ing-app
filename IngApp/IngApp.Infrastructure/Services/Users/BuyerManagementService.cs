using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Interfaces.Users;
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Users.DTO;
using IngApp.Domain.Entities.Users;
using IngApp.Infrastructure.Persistence;
using IngApp.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace IngApp.Infrastructure.Services.Users;

public class BuyerManagementService : IBuyerManagementService
{
    private readonly AppDbContext _db;
    private readonly IVisitorProfileService _visitorProfileService;

    public BuyerManagementService(AppDbContext db, IVisitorProfileService visitorProfileService)
    {
        _db = db;
        _visitorProfileService = visitorProfileService;
    }

    public async Task<PagedResult<BuyerManagementDto>> GetPagedAsync(BuyerListQueryDto filter)
    {
        var query = _db.BuyerProfiles
            .Include(bp => bp.User)
            .Include(bp => bp.ReferredByVisitor)
                .ThenInclude(v => v.User)
            .AsQueryable();

        // فیلتر جستجو
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            query = query.Where(bp =>
                bp.User.PhoneNumber.Contains(search) ||
                (bp.User.DisplayName != null && bp.User.DisplayName.Contains(search)) ||
                (bp.BusinessName != null && bp.BusinessName.Contains(search)) ||
                (bp.ContactMobile != null && bp.ContactMobile.Contains(search)));
        }

        // فیلتر بر اساس بازاریاب
        if (filter.ReferredByVisitorId.HasValue)
        {
            query = query.Where(bp => bp.ReferredByVisitorId == filter.ReferredByVisitorId.Value);
        }

        // فیلتر بر اساس کد معرف
        if (!string.IsNullOrWhiteSpace(filter.ReferralCode))
        {
            var referralCode = filter.ReferralCode.Trim().ToUpper();
            query = query.Where(bp =>
                bp.ReferredByVisitor != null &&
                bp.ReferredByVisitor.ReferralCode == referralCode);
        }

        // مرتب‌سازی
        query = (filter.SortBy?.ToLower(), filter.SortDesc) switch
        {
            ("phonenumber", true) => query.OrderByDescending(bp => bp.User.PhoneNumber),
            ("phonenumber", false) => query.OrderBy(bp => bp.User.PhoneNumber),
            ("displayname", true) => query.OrderByDescending(bp => bp.User.DisplayName),
            ("displayname", false) => query.OrderBy(bp => bp.User.DisplayName),
            ("businessname", true) => query.OrderByDescending(bp => bp.BusinessName),
            ("businessname", false) => query.OrderBy(bp => bp.BusinessName),
            ("referralcode", true) => query.OrderByDescending(bp => bp.ReferredByVisitor != null ? bp.ReferredByVisitor.ReferralCode : ""),
            ("referralcode", false) => query.OrderBy(bp => bp.ReferredByVisitor != null ? bp.ReferredByVisitor.ReferralCode : ""),
            ("createdat", false) => query.OrderBy(bp => bp.CreatedAt),
            _ => query.OrderByDescending(bp => bp.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var page = filter.Page <= 0 ? 1 : filter.Page;
        var pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

        var buyers = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtoList = buyers.Select(bp => new BuyerManagementDto
        {
            Id = bp.Id,
            UserId = bp.UserId,
            UserPhoneNumber = bp.User.PhoneNumber,
            UserDisplayName = bp.User.DisplayName,
            BusinessName = bp.BusinessName,
            ContactMobile = bp.ContactMobile,
            Province = bp.Province,
            City = bp.City,
            Address = bp.Address,
            Description = bp.Description,
            ReferredByVisitorId = bp.ReferredByVisitorId,
            ReferredByVisitorCode = bp.ReferredByVisitor?.ReferralCode,
            ReferredByVisitorName = bp.ReferredByVisitor?.BusinessName ?? bp.ReferredByVisitor?.User.DisplayName,
            ReferredByVisitorPhoneNumber = bp.ReferredByVisitor?.User.PhoneNumber,
            CreatedAt = bp.CreatedAt,
            UpdatedAt = bp.UpdatedAt
        }).ToList();

        return new PagedResult<BuyerManagementDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = dtoList
        };
    }

    public async Task<BuyerManagementDto?> GetByIdAsync(Guid buyerProfileId)
    {
        var buyer = await _db.BuyerProfiles
            .Include(bp => bp.User)
            .Include(bp => bp.ReferredByVisitor)
                .ThenInclude(v => v.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(bp => bp.Id == buyerProfileId);

        if (buyer == null)
            return null;

        return new BuyerManagementDto
        {
            Id = buyer.Id,
            UserId = buyer.UserId,
            UserPhoneNumber = buyer.User.PhoneNumber,
            UserDisplayName = buyer.User.DisplayName,
            BusinessName = buyer.BusinessName,
            ContactMobile = buyer.ContactMobile,
            Province = buyer.Province,
            City = buyer.City,
            Address = buyer.Address,
            Description = buyer.Description,
            ReferredByVisitorId = buyer.ReferredByVisitorId,
            ReferredByVisitorCode = buyer.ReferredByVisitor?.ReferralCode,
            ReferredByVisitorName = buyer.ReferredByVisitor?.BusinessName ?? buyer.ReferredByVisitor?.User.DisplayName,
            ReferredByVisitorPhoneNumber = buyer.ReferredByVisitor?.User.PhoneNumber,
            CreatedAt = buyer.CreatedAt,
            UpdatedAt = buyer.UpdatedAt
        };
    }

    public async Task<BuyerManagementDto> CreateAsync(CreateBuyerDto dto)
    {
        var phone = dto.PhoneNumber?.Trim();
        if (string.IsNullOrWhiteSpace(phone))
            throw new ValidationException(new() { "شماره موبایل اجباری است." });

        // بررسی یا ایجاد User
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == phone);

        if (user == null)
        {
            // دریافت UserType برای Buyer
            var buyerUserType = await _db.UserTypes
                .FirstOrDefaultAsync(ut => ut.Code == "Buyer" && ut.IsActive);

            if (buyerUserType == null)
                throw new AppException("نوع کاربر Buyer در سیستم یافت نشد.");

            user = new User
            {
                PhoneNumber = phone,
                DisplayName = dto.DisplayName?.Trim(),
                UserTypeId = buyerUserType.Id,
                IsActive = true
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // اضافه کردن نقش Buyer به User جدید
            var buyerRoleId = RoleConfiguration.BuyerRoleId;
            var hasBuyerRole = await _db.UserRoles
                .AnyAsync(x => x.UserId == user.Id && x.RoleId == buyerRoleId);

            if (!hasBuyerRole)
            {
                _db.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = buyerRoleId
                });
                await _db.SaveChangesAsync();
            }
        }

        // بررسی اینکه BuyerProfile ندارد
        var existingProfile = await _db.BuyerProfiles
            .FirstOrDefaultAsync(bp => bp.UserId == user.Id);

        if (existingProfile != null)
            throw new ValidationException(new() { "این کاربر قبلاً BuyerProfile دارد." });

        // پیدا کردن بازاریاب
        Guid? referredByVisitorId = null;
        if (dto.ReferredByVisitorId.HasValue)
        {
            var visitor = await _db.VisitorProfiles
                .Include(vp => vp.User)
                .FirstOrDefaultAsync(vp => vp.Id == dto.ReferredByVisitorId.Value && vp.IsActive);
            
            if (visitor == null)
                throw new NotFoundException("بازاریاب انتخاب شده یافت نشد یا غیرفعال است.");

            // بررسی اینکه Buyer خودش را به عنوان بازاریاب انتخاب نکرده باشد
            if (visitor.UserId == user.Id)
                throw new ValidationException(new() { "یک کاربر نمی‌تواند خودش را به عنوان بازاریاب خودش انتخاب کند." });

            referredByVisitorId = visitor.Id;
        }
        else if (!string.IsNullOrWhiteSpace(dto.ReferralCode))
        {
            var visitorProfile = await _visitorProfileService.GetByReferralCodeAsync(dto.ReferralCode.Trim().ToUpper());
            
            if (visitorProfile == null)
                throw new NotFoundException($"کد معرف '{dto.ReferralCode}' یافت نشد.");

            if (!visitorProfile.IsActive)
                throw new ValidationException(new() { $"کد معرف '{dto.ReferralCode}' غیرفعال است." });

            // بررسی اینکه Buyer خودش را به عنوان بازاریاب انتخاب نکرده باشد
            if (visitorProfile.UserId == user.Id)
                throw new ValidationException(new() { "یک کاربر نمی‌تواند خودش را به عنوان بازاریاب خودش انتخاب کند." });

            referredByVisitorId = visitorProfile.Id;
        }

        // ایجاد BuyerProfile
        var buyerProfile = new BuyerProfile
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            BusinessName = dto.BusinessName?.Trim(),
            ContactMobile = dto.ContactMobile?.Trim(),
            Province = dto.Province?.Trim(),
            City = dto.City?.Trim(),
            Address = dto.Address?.Trim(),
            Description = dto.Description?.Trim(),
            ReferredByVisitorId = referredByVisitorId,
            CreatedAt = DateTime.UtcNow
        };

        _db.BuyerProfiles.Add(buyerProfile);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(buyerProfile.Id)
            ?? throw new AppException("خطا در ایجاد BuyerProfile");
    }

    public async Task<BuyerManagementDto> UpdateAsync(Guid buyerProfileId, UpdateBuyerDto dto)
    {
        var buyer = await _db.BuyerProfiles
            .FirstOrDefaultAsync(bp => bp.Id == buyerProfileId);

        if (buyer == null)
            throw new NotFoundException("خریدار یافت نشد.");

        buyer.BusinessName = dto.BusinessName?.Trim();
        buyer.ContactMobile = dto.ContactMobile?.Trim();
        buyer.Province = dto.Province?.Trim();
        buyer.City = dto.City?.Trim();
        buyer.Address = dto.Address?.Trim();
        buyer.Description = dto.Description?.Trim();
        buyer.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return await GetByIdAsync(buyerProfileId)
            ?? throw new AppException("خطا در به‌روزرسانی BuyerProfile");
    }

    public async Task<BuyerManagementDto> SetReferralAsync(Guid buyerProfileId, SetBuyerReferralDto dto)
    {
        var buyer = await _db.BuyerProfiles
            .FirstOrDefaultAsync(bp => bp.Id == buyerProfileId);

        if (buyer == null)
            throw new NotFoundException("خریدار یافت نشد.");

        Guid? referredByVisitorId = null;

        if (dto.ReferredByVisitorId.HasValue)
        {
            var visitor = await _db.VisitorProfiles
                .Include(vp => vp.User)
                .FirstOrDefaultAsync(vp => vp.Id == dto.ReferredByVisitorId.Value && vp.IsActive);
            
            if (visitor == null)
                throw new NotFoundException("بازاریاب انتخاب شده یافت نشد یا غیرفعال است.");

            // بررسی اینکه Buyer خودش را به عنوان بازاریاب انتخاب نکرده باشد
            if (visitor.UserId == buyer.UserId)
                throw new ValidationException(new() { "یک کاربر نمی‌تواند خودش را به عنوان بازاریاب خودش انتخاب کند." });

            referredByVisitorId = visitor.Id;
        }
        else if (!string.IsNullOrWhiteSpace(dto.ReferralCode))
        {
            var visitorProfile = await _visitorProfileService.GetByReferralCodeAsync(dto.ReferralCode.Trim().ToUpper());
            
            if (visitorProfile == null)
                throw new NotFoundException($"کد معرف '{dto.ReferralCode}' یافت نشد.");

            if (!visitorProfile.IsActive)
                throw new ValidationException(new() { $"کد معرف '{dto.ReferralCode}' غیرفعال است." });

            // بررسی اینکه Buyer خودش را به عنوان بازاریاب انتخاب نکرده باشد
            if (visitorProfile.UserId == buyer.UserId)
                throw new ValidationException(new() { "یک کاربر نمی‌تواند خودش را به عنوان بازاریاب خودش انتخاب کند." });

            referredByVisitorId = visitorProfile.Id;
        }

        buyer.ReferredByVisitorId = referredByVisitorId;
        buyer.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return await GetByIdAsync(buyerProfileId)
            ?? throw new AppException("خطا در به‌روزرسانی بازاریاب");
    }

    public async Task RemoveReferralAsync(Guid buyerProfileId)
    {
        var buyer = await _db.BuyerProfiles
            .FirstOrDefaultAsync(bp => bp.Id == buyerProfileId);

        if (buyer == null)
            throw new NotFoundException("خریدار یافت نشد.");

        buyer.ReferredByVisitorId = null;
        buyer.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid buyerProfileId)
    {
        var buyer = await _db.BuyerProfiles
            .FirstOrDefaultAsync(bp => bp.Id == buyerProfileId);

        if (buyer == null)
            throw new NotFoundException("خریدار یافت نشد.");

        _db.BuyerProfiles.Remove(buyer);
        await _db.SaveChangesAsync();
    }
}

