using IngApp.Application.Features.Users.DTO;

namespace IngApp.Application.Common.Interfaces.Users;

public interface IBuyerProfileService
{
    /// <summary>
    /// دریافت پروفایل Buyer برای کاربر فعلی
    /// </summary>
    Task<BuyerProfileDto?> GetMyProfileAsync(Guid userId);
    
    /// <summary>
    /// ایجاد یا به‌روزرسانی پروفایل Buyer برای کاربر فعلی
    /// </summary>
    Task<BuyerProfileDto> UpsertMyProfileAsync(Guid userId, UpsertBuyerProfileDto dto);
    
    /// <summary>
    /// دریافت پروفایل Buyer بر اساس UserId
    /// </summary>
    Task<BuyerProfileDto?> GetByUserIdAsync(Guid userId);
}











