using IngApp.Domain.Enums;

namespace IngApp.Application.Features.Users.DTO
{
    public class UserDto
    {
        public Guid Id { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;
        public string? DisplayName { get; set; }

        // UserType از Lookup Table
        public string UserTypeCode { get; set; } = string.Empty;
        public string UserTypeTitle { get; set; } = string.Empty;
        
        public SubscriptionLevel SubscriptionLevel { get; set; }
        public VerificationStatus VerificationStatus { get; set; }

        public bool IsActive { get; set; }

        // نقش‌ها به صورت نام نقش (Role.Name)
        public List<string> Roles { get; set; } = new();

        // ---------- نام انگلیسی Enum ها ----------
        public string UserTypeName => UserTypeCode;
        public string SubscriptionLevelName => SubscriptionLevel.ToString();
        public string VerificationStatusName => VerificationStatus.ToString();

        // ---------- نام فارسی Enum ها برای UI ----------
        public string UserTypeFa => UserTypeTitle;

        public string SubscriptionLevelFa => SubscriptionLevel switch
        {
            SubscriptionLevel.None => "بدون اشتراک",
            SubscriptionLevel.Bronze => "برنزی",
            SubscriptionLevel.Silver => "نقره‌ای",
            SubscriptionLevel.Gold => "طلایی",
            _ => SubscriptionLevel.ToString()
        };

        public string VerificationStatusFa => VerificationStatus switch
        {
            VerificationStatus.NotSubmitted => "ارسال نشده",
            VerificationStatus.Pending => "در انتظار بررسی",
            VerificationStatus.Approved => "تأیید شده",
            VerificationStatus.Rejected => "رد شده",
            _ => VerificationStatus.ToString()
        };
    }
}
