using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Interfaces.Financial;
using IngApp.Application.Features.Financial.DTO;
using IngApp.Domain.Entities.Financial;
using IngApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IngApp.Infrastructure.Services.Financial;

public class CommissionRuleService : ICommissionRuleService
{
    private readonly AppDbContext _db;

    public CommissionRuleService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<CommissionRuleDto>> GetAllAsync()
    {
        return await _db.CommissionRules
            .OrderBy(cr => cr.Code)
            .Select(cr => new CommissionRuleDto
            {
                Id = cr.Id,
                Code = cr.Code,
                Title = cr.Title,
                Description = cr.Description,
                CommissionPercentage = cr.CommissionPercentage,
                IsActive = cr.IsActive,
                EffectiveFrom = cr.EffectiveFrom,
                EffectiveTo = cr.EffectiveTo,
                CreatedAt = cr.CreatedAt,
                UpdatedAt = cr.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<CommissionRuleDto?> GetByIdAsync(int id)
    {
        var rule = await _db.CommissionRules
            .FirstOrDefaultAsync(cr => cr.Id == id);

        if (rule == null)
            return null;

        return new CommissionRuleDto
        {
            Id = rule.Id,
            Code = rule.Code,
            Title = rule.Title,
            Description = rule.Description,
            CommissionPercentage = rule.CommissionPercentage,
            IsActive = rule.IsActive,
            EffectiveFrom = rule.EffectiveFrom,
            EffectiveTo = rule.EffectiveTo,
            CreatedAt = rule.CreatedAt,
            UpdatedAt = rule.UpdatedAt
        };
    }

    public async Task<CommissionRuleDto?> GetByCodeAsync(string code)
    {
        var rule = await _db.CommissionRules
            .FirstOrDefaultAsync(cr => cr.Code == code);

        if (rule == null)
            return null;

        return new CommissionRuleDto
        {
            Id = rule.Id,
            Code = rule.Code,
            Title = rule.Title,
            Description = rule.Description,
            CommissionPercentage = rule.CommissionPercentage,
            IsActive = rule.IsActive,
            EffectiveFrom = rule.EffectiveFrom,
            EffectiveTo = rule.EffectiveTo,
            CreatedAt = rule.CreatedAt,
            UpdatedAt = rule.UpdatedAt
        };
    }

    public async Task<CommissionRuleDto> CreateAsync(CreateCommissionRuleDto dto)
    {
        // بررسی تکراری نبودن Code
        var exists = await _db.CommissionRules
            .AnyAsync(cr => cr.Code == dto.Code.Trim());
        if (exists)
            throw new ValidationException(new() { $"قانون پورسانت با کد '{dto.Code}' قبلاً ثبت شده است." });

        // بررسی اعتبار تاریخ‌ها
        if (dto.EffectiveFrom.HasValue && dto.EffectiveTo.HasValue)
        {
            if (dto.EffectiveFrom.Value > dto.EffectiveTo.Value)
                throw new ValidationException(new() { "تاریخ شروع باید قبل از تاریخ پایان باشد." });
        }

        // جلوگیری از هم‌پوشانی بازه‌های زمانی برای یک Code
        // فقط قوانین فعال را در نظر می‌گیریم
        var hasOverlap = await _db.CommissionRules.AnyAsync(cr =>
            cr.Code == dto.Code.Trim() &&
            cr.IsActive &&
            // اگر قانون قبلی بازه‌ای دارد که با بازه جدید تداخل دارد
            (cr.EffectiveFrom == null || dto.EffectiveTo == null || cr.EffectiveFrom <= dto.EffectiveTo) &&
            (dto.EffectiveFrom == null || cr.EffectiveTo == null || cr.EffectiveTo >= dto.EffectiveFrom));

        if (hasOverlap)
        {
            throw new ValidationException(new()
            {
                "برای این کد قانون، در بازه‌ی زمانی انتخاب‌شده قبلاً قانون دیگری ثبت شده است."
            });
        }

        var rule = new CommissionRule
        {
            Code = dto.Code.Trim(),
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            CommissionPercentage = dto.CommissionPercentage,
            IsActive = dto.IsActive,
            EffectiveFrom = dto.EffectiveFrom,
            EffectiveTo = dto.EffectiveTo,
            CreatedAt = DateTime.UtcNow
        };

        _db.CommissionRules.Add(rule);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(rule.Id) ?? throw new AppException("خطا در واکشی قانون پورسانت بعد از ایجاد.");
    }

    public async Task<CommissionRuleDto> UpdateAsync(int id, UpdateCommissionRuleDto dto)
    {
        var rule = await _db.CommissionRules
            .FirstOrDefaultAsync(cr => cr.Id == id);

        if (rule == null)
            throw new NotFoundException("قانون پورسانت یافت نشد.");

        // بررسی اعتبار تاریخ‌ها
        if (dto.EffectiveFrom.HasValue && dto.EffectiveTo.HasValue)
        {
            if (dto.EffectiveFrom.Value > dto.EffectiveTo.Value)
                throw new ValidationException(new() { "تاریخ شروع باید قبل از تاریخ پایان باشد." });
        }

        // جلوگیری از هم‌پوشانی بازه‌های زمانی برای یک Code (به‌جز همین قانون)
        var hasOverlap = await _db.CommissionRules.AnyAsync(cr =>
            cr.Id != rule.Id &&
            cr.Code == rule.Code &&
            cr.IsActive &&
            (cr.EffectiveFrom == null || dto.EffectiveTo == null || cr.EffectiveFrom <= dto.EffectiveTo) &&
            (dto.EffectiveFrom == null || cr.EffectiveTo == null || cr.EffectiveTo >= dto.EffectiveFrom));

        if (hasOverlap)
        {
            throw new ValidationException(new()
            {
                "برای این کد قانون، در بازه‌ی زمانی انتخاب‌شده قبلاً قانون دیگری ثبت شده است."
            });
        }

        // Code قابل تغییر نیست (چون در سیستم استفاده می‌شود)
        rule.Title = dto.Title.Trim();
        rule.Description = dto.Description?.Trim();
        rule.CommissionPercentage = dto.CommissionPercentage;
        rule.IsActive = dto.IsActive;
        rule.EffectiveFrom = dto.EffectiveFrom;
        rule.EffectiveTo = dto.EffectiveTo;
        rule.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return await GetByIdAsync(rule.Id) ?? throw new AppException("خطا در واکشی قانون پورسانت بعد از به‌روزرسانی.");
    }

    public async Task DeleteAsync(int id)
    {
        var rule = await _db.CommissionRules
            .FirstOrDefaultAsync(cr => cr.Id == id);
        if (rule == null)
            throw new NotFoundException("قانون پورسانت یافت نشد.");

        // Soft Delete: فقط غیرفعال می‌کنیم تا تاریخچه حفظ شود
        rule.IsActive = false;

        // اگر تاریخ پایان تنظیم نشده یا در آینده است، آن را الان قرار می‌دهیم
        var now = DateTime.UtcNow;
        if (!rule.EffectiveTo.HasValue || rule.EffectiveTo.Value > now)
        {
            rule.EffectiveTo = now;
        }

        rule.UpdatedAt = now;

        await _db.SaveChangesAsync();
    }
}

