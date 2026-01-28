using IngApp.Domain.Enums;

namespace IngApp.Application.Features.Users.DTO
{
    public class UpdateUserDto
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string? DisplayName { get; set; }

        /// <summary>
        /// نوع کاربر (Code: Buyer, Supplier, Admin, Visitor)
        /// </summary>
        public string UserTypeCode { get; set; } = string.Empty;
        
        public SubscriptionLevel SubscriptionLevel { get; set; }
        public VerificationStatus VerificationStatus { get; set; }
    }
}
