// IngApp.Application/Features/Users/DTO/UserListQueryDto.cs
using IngApp.Domain.Enums;

namespace IngApp.Application.Features.Users.DTO
{
    public class UserListQueryDto
    {
        // صفحه فعلی (۱، ۲، ۳، ...)
        public int Page { get; set; } = 1;

        // تعداد در هر صفحه
        public int PageSize { get; set; } = 10;

        // نام ستون برای sort
        // مثال: "phoneNumber", "displayName", "userType", "subscriptionLevel", "verificationStatus", "createdAt"
        public string? SortBy { get; set; }

        // اگر true باشد، نزولی (DESC) – پیش‌فرض صعودی
        public bool SortDesc { get; set; }

        // --------- فیلترها ---------
        public string? PhoneNumber { get; set; }
        public string? DisplayName { get; set; }

        /// <summary>
        /// فیلتر بر اساس نوع کاربر (Code: Buyer, Supplier, Admin, Visitor)
        /// </summary>
        public string? UserTypeCode { get; set; }
        public SubscriptionLevel? SubscriptionLevel { get; set; }
        public VerificationStatus? VerificationStatus { get; set; }

        // فیلتر بر اساس نقش (Role)
        public Guid? RoleId { get; set; }
    }
}
