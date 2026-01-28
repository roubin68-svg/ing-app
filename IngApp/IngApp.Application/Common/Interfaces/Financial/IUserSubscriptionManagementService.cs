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
}










