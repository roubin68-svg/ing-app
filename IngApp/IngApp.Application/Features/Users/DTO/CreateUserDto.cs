using IngApp.Domain.Enums;

namespace IngApp.Application.Features.Users.DTO
{
    public class CreateUserDto
    {
        /// <summary>
        /// شماره موبایل کاربر (یونیک، برای لاگین)
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// نام نمایشی (مثلاً نام کسب‌وکار یا نام کاربری)
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// نوع کاربر (خریدار / تأمین‌کننده / ادمین)
        /// </summary>
        public UserType UserType { get; set; } = UserType.Buyer;

        /// <summary>
        /// سطح اشتراک
        /// </summary>
        public SubscriptionLevel SubscriptionLevel { get; set; } = SubscriptionLevel.None;

        /// <summary>
        /// وضعیت احراز هویت
        /// </summary>
        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.NotSubmitted;

        /// <summary>
        /// فعال / غیرفعال
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// نقش‌های اولیه کاربر (اختیاری)
        /// </summary>
        public List<Guid> RoleIds { get; set; } = new();
    }
}
