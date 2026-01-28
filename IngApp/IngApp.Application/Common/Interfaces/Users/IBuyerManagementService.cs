using IngApp.Application.Common.Models;
using IngApp.Application.Features.Users.DTO;

namespace IngApp.Application.Common.Interfaces.Users;

/// <summary>
/// سرویس مدیریت Buyer ها توسط Admin
/// </summary>
public interface IBuyerManagementService
{
    /// <summary>
    /// دریافت لیست صفحه‌بندی‌شده Buyer ها
    /// </summary>
    Task<PagedResult<BuyerManagementDto>> GetPagedAsync(BuyerListQueryDto filter);

    /// <summary>
    /// دریافت Buyer بر اساس Id
    /// </summary>
    Task<BuyerManagementDto?> GetByIdAsync(Guid buyerProfileId);

    /// <summary>
    /// ایجاد Buyer جدید (ایجاد User و BuyerProfile)
    /// </summary>
    Task<BuyerManagementDto> CreateAsync(CreateBuyerDto dto);

    /// <summary>
    /// به‌روزرسانی اطلاعات Buyer
    /// </summary>
    Task<BuyerManagementDto> UpdateAsync(Guid buyerProfileId, UpdateBuyerDto dto);

    /// <summary>
    /// تنظیم یا تغییر بازاریاب برای Buyer (با کد معرف یا انتخاب از لیست)
    /// </summary>
    Task<BuyerManagementDto> SetReferralAsync(Guid buyerProfileId, SetBuyerReferralDto dto);

    /// <summary>
    /// حذف بازاریاب از Buyer
    /// </summary>
    Task RemoveReferralAsync(Guid buyerProfileId);

    /// <summary>
    /// حذف Buyer
    /// </summary>
    Task DeleteAsync(Guid buyerProfileId);
}


