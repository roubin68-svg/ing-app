using IngApp.Application.Common.Models;
using IngApp.Application.Features.Users.DTO;

namespace IngApp.Application.Common.Interfaces.Users;

/// <summary>
/// سرویس مدیریت Visitor ها توسط Admin
/// </summary>
public interface IVisitorManagementService
{
    /// <summary>
    /// دریافت لیست Visitor ها با فیلتر و صفحه‌بندی
    /// </summary>
    Task<PagedResult<VisitorManagementDto>> GetPagedAsync(VisitorListQueryDto filter);
    
    /// <summary>
    /// دریافت Visitor بر اساس ID
    /// </summary>
    Task<VisitorManagementDto?> GetByIdAsync(Guid visitorProfileId);
    
    /// <summary>
    /// ایجاد Visitor جدید برای یک User
    /// </summary>
    Task<VisitorManagementDto> CreateAsync(CreateVisitorDto dto);
    
    /// <summary>
    /// ویرایش Visitor
    /// </summary>
    Task<VisitorManagementDto> UpdateAsync(Guid visitorProfileId, UpdateVisitorDto dto);
    
    /// <summary>
    /// تغییر وضعیت فعال/غیرفعال Visitor
    /// </summary>
    Task ChangeStatusAsync(Guid visitorProfileId, bool isActive);
    
    /// <summary>
    /// حذف Visitor
    /// </summary>
    Task DeleteAsync(Guid visitorProfileId);
    
    /// <summary>
    /// دریافت لیست Buyer های یک Visitor
    /// </summary>
    Task<List<BuyerForVisitorDto>> GetBuyersAsync(Guid visitorProfileId);
    
    /// <summary>
    /// اضافه کردن Buyer به Visitor (با Mobile)
    /// </summary>
    Task<BuyerForVisitorDto> AddBuyerAsync(Guid visitorProfileId, AddBuyerToVisitorDto dto);
    
    /// <summary>
    /// حذف Buyer از Visitor (حذف ReferralCode)
    /// </summary>
    Task RemoveBuyerAsync(Guid visitorProfileId, Guid buyerProfileId);
    
    /// <summary>
    /// دریافت Commission Rules یک Visitor
    /// </summary>
    Task<List<VisitorCommissionRuleDto>> GetCommissionRulesAsync(Guid visitorProfileId);
    
    /// <summary>
    /// تنظیم Commission Rule برای Visitor
    /// </summary>
    Task<VisitorCommissionRuleDto> SetCommissionRuleAsync(Guid visitorProfileId, SetVisitorCommissionRuleDto dto);
    
    /// <summary>
    /// حذف Commission Rule از Visitor (بازگشت به پیش‌فرض)
    /// </summary>
    Task RemoveCommissionRuleAsync(Guid visitorProfileId, string commissionRuleCode);
}










