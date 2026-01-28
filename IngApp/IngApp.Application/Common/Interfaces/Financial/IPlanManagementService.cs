using IngApp.Application.Common.Models;
using IngApp.Application.Features.Financial.DTO;

namespace IngApp.Application.Common.Interfaces.Financial;

/// <summary>
/// Service برای مدیریت Plan های اشتراک (Admin)
/// </summary>
public interface IPlanManagementService
{
    /// <summary>
    /// دریافت لیست تمام Plan ها (با Pagination)
    /// </summary>
    Task<PagedResult<PlanDto>> GetPagedPlansAsync(int page = 1, int pageSize = 20);

    /// <summary>
    /// دریافت تمام Plan ها
    /// </summary>
    Task<List<PlanDto>> GetAllPlansAsync();

    /// <summary>
    /// دریافت Plan بر اساس Id
    /// </summary>
    Task<PlanDto?> GetPlanByIdAsync(int id);

    /// <summary>
    /// ایجاد Plan جدید
    /// </summary>
    Task<int> CreatePlanAsync(CreatePlanDto dto);

    /// <summary>
    /// به‌روزرسانی Plan
    /// </summary>
    Task UpdatePlanAsync(int id, UpdatePlanDto dto);

    /// <summary>
    /// تغییر وضعیت فعال/غیرفعال Plan
    /// </summary>
    Task TogglePlanStatusAsync(int id, bool isActive);

    /// <summary>
    /// حذف Plan (فقط اگر هیچ اشتراک فعالی نداشته باشد)
    /// </summary>
    Task DeletePlanAsync(int id);
}










