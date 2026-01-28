using IngApp.Application.Features.Users.DTO;

namespace IngApp.Application.Common.Interfaces.Users;

public interface IVisitorProfileService
{
    /// <summary>
    /// دریافت پروفایل Visitor برای کاربر فعلی
    /// </summary>
    Task<VisitorProfileDto?> GetMyProfileAsync(Guid userId);
    
    /// <summary>
    /// ایجاد یا به‌روزرسانی پروفایل Visitor برای کاربر فعلی
    /// </summary>
    Task<VisitorProfileDto> UpsertMyProfileAsync(Guid userId, UpsertVisitorProfileDto dto);
    
    /// <summary>
    /// دریافت پروفایل Visitor بر اساس ReferralCode
    /// </summary>
    Task<VisitorProfileDto?> GetByReferralCodeAsync(string referralCode);
    
    /// <summary>
    /// دریافت پروفایل Visitor بر اساس UserId
    /// </summary>
    Task<VisitorProfileDto?> GetByUserIdAsync(Guid userId);
}










