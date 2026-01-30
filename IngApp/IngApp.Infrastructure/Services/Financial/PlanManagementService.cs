using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Interfaces.Financial;
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Financial.DTO;
using IngApp.Domain.Entities.Financial;
using IngApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IngApp.Infrastructure.Services.Financial;

public class PlanManagementService : IPlanManagementService
{
    private readonly AppDbContext _db;

    public PlanManagementService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<PlanDto>> GetPagedPlansAsync(int page = 1, int pageSize = 20)
    {
        var query = _db.Plans.AsNoTracking();

        var totalCount = await query.CountAsync();

        var plans = await query
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.DurationMonths)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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

        return new PagedResult<PlanDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = plans
        };
    }

    public async Task<List<PlanDto>> GetAllPlansAsync()
    {
        return await _db.Plans
            .AsNoTracking()
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.DurationMonths)
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

    public async Task<PlanDto?> GetPlanByIdAsync(int id)
    {
        var plan = await _db.Plans
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (plan == null)
            return null;

        return new PlanDto
        {
            Id = plan.Id,
            Code = plan.Code,
            Title = plan.Title,
            Description = plan.Description,
            DurationMonths = plan.DurationMonths,
            PriceRial = plan.PriceRial,
            UnlimitedContactViews = plan.UnlimitedContactViews,
            IsActive = plan.IsActive,
            DisplayOrder = plan.DisplayOrder
        };
    }

    public async Task<int> CreatePlanAsync(CreatePlanDto dto)
    {
        // بررسی Code تکراری
        var existingPlan = await _db.Plans
            .FirstOrDefaultAsync(p => p.Code == dto.Code);

        if (existingPlan != null)
        {
            throw new ValidationException(new() { "کد پلن تکراری است." });
        }

        var plan = new Plan
        {
            Code = dto.Code,
            Title = dto.Title,
            Description = dto.Description,
            DurationMonths = dto.DurationMonths,
            PriceRial = dto.PriceRial,
            UnlimitedContactViews = dto.UnlimitedContactViews,
            IsActive = dto.IsActive,
            DisplayOrder = dto.DisplayOrder,
            CreatedAt = DateTime.Now
        };

        _db.Plans.Add(plan);
        await _db.SaveChangesAsync();

        return plan.Id;
    }

    public async Task UpdatePlanAsync(int id, UpdatePlanDto dto)
    {
        var plan = await _db.Plans.FirstOrDefaultAsync(p => p.Id == id);

        if (plan == null)
        {
            throw new NotFoundException("پلن مورد نظر یافت نشد.");
        }

        // بررسی Code تکراری (به جز خود Plan)
        var existingPlan = await _db.Plans
            .FirstOrDefaultAsync(p => p.Code == dto.Code && p.Id != id);

        if (existingPlan != null)
        {
            throw new ValidationException(new() { "کد پلن تکراری است." });
        }

        plan.Code = dto.Code;
        plan.Title = dto.Title;
        plan.Description = dto.Description;
        plan.DurationMonths = dto.DurationMonths;
        plan.PriceRial = dto.PriceRial;
        plan.UnlimitedContactViews = dto.UnlimitedContactViews;
        plan.IsActive = dto.IsActive;
        plan.DisplayOrder = dto.DisplayOrder;
        plan.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();
    }

    public async Task TogglePlanStatusAsync(int id, bool isActive)
    {
        var plan = await _db.Plans.FirstOrDefaultAsync(p => p.Id == id);

        if (plan == null)
        {
            throw new NotFoundException("پلن مورد نظر یافت نشد.");
        }

        plan.IsActive = isActive;
        plan.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();
    }

    public async Task DeletePlanAsync(int id)
    {
        var plan = await _db.Plans
            .Include(p => p.UserSubscriptions)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (plan == null)
        {
            throw new NotFoundException("پلن مورد نظر یافت نشد.");
        }

        // بررسی اینکه آیا اشتراک فعالی دارد یا نه
        var activeStatus = await _db.SubscriptionStatuses
            .FirstAsync(s => s.Code == "Active");

        var hasActiveSubscriptions = plan.UserSubscriptions
            .Any(us => us.StatusId == activeStatus.Id);

        if (hasActiveSubscriptions)
        {
            throw new ValidationException(new() { "امکان حذف پلنی که اشتراک فعال دارد وجود ندارد." });
        }

        _db.Plans.Remove(plan);
        await _db.SaveChangesAsync();
    }
}












