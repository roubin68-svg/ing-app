using IngApp.Domain.Enums;

namespace IngApp.Application.Features.Users.DTO
{
    public class UpdateUserDto
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string? DisplayName { get; set; }

        public UserType UserType { get; set; }
        public SubscriptionLevel SubscriptionLevel { get; set; }
        public VerificationStatus VerificationStatus { get; set; }
    }
}
