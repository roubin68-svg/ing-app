using IngApp.Application.Common.Models;
using IngApp.Application.Features.Financial.DTO;

namespace IngApp.Application.Common.Interfaces.Financial;

/// <summary>
/// Service برای مدیریت اشتراک‌های خریداری شده (Admin)
/// </summary>
public interface IUserSubscriptionManagementService
{
    /// <summary>
    /// دریافت لیست اشتراک‌ها با Pagination و فیلتر
    /// </summary>
    Task<PagedResult<UserSubscriptionDetailDto>> GetPagedSubscriptionsAsync(UserSubscriptionListQueryDto query);

    /// <summary>
    /// دریافت لیست کاربران با خلاصه اشتراک‌ها
    /// </summary>
    Task<PagedResult<UserWithSubscriptionsSummaryDto>> GetUsersWithSubscriptionsSummaryAsync(UsersWithSubscriptionsQueryDto query);

    /// <summary>
    /// ویرایش اشتراک کاربر
    /// </summary>
    Task<UserSubscriptionDetailDto> UpdateSubscriptionAsync(Guid subscriptionId, UpdateUserSubscriptionDto dto);
}


















