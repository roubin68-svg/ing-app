using IngApp.Application.Common.Interfaces.Financial;
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Financial.DTO;
using IngApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IngApp.Infrastructure.Services.Financial;

public class UserSubscriptionManagementService : IUserSubscriptionManagementService
{
    private readonly AppDbContext _db;

    public UserSubscriptionManagementService(AppDbContext db)
    {
        _db = db;
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
}










