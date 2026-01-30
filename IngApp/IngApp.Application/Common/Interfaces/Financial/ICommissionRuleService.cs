using IngApp.Application.Common.Models;
using IngApp.Application.Features.Financial.DTO;

namespace IngApp.Application.Common.Interfaces.Financial;

/// <summary>
/// Service برای مدیریت قوانین پورسانت پیش‌فرض
/// </summary>
public interface ICommissionRuleService
{
    /// <summary>
    /// دریافت لیست تمام قوانین پورسانت
    /// </summary>
    Task<List<CommissionRuleDto>> GetAllAsync();

    /// <summary>
    /// دریافت یک قانون پورسانت بر اساس ID
    /// </summary>
    Task<CommissionRuleDto?> GetByIdAsync(int id);

    /// <summary>
    /// دریافت یک قانون پورسانت بر اساس Code
    /// </summary>
    Task<CommissionRuleDto?> GetByCodeAsync(string code);

    /// <summary>
    /// ایجاد قانون پورسانت جدید
    /// </summary>
    Task<CommissionRuleDto> CreateAsync(CreateCommissionRuleDto dto);

    /// <summary>
    /// به‌روزرسانی قانون پورسانت
    /// </summary>
    Task<CommissionRuleDto> UpdateAsync(int id, UpdateCommissionRuleDto dto);

    /// <summary>
    /// حذف قانون پورسانت
    /// </summary>
    Task DeleteAsync(int id);
}











