using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IngApp.Domain.Entities.Kyc;
using IngApp.Domain.Enums;

namespace IngApp.Domain.Entities.Users;

public class User
{
    public Guid Id { get; set; }

    public string PhoneNumber { get; set; } = string.Empty; // یکتا، برای OTP Login
    public string? DisplayName { get; set; }                // نام کسب‌وکار / نام کاربری

    public UserType UserType { get; set; }
    public SubscriptionLevel SubscriptionLevel { get; set; } = SubscriptionLevel.None;

    public bool IsActive { get; set; } = true;

    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.NotSubmitted;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // ناوبری‌ها
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<UserDocument> Documents { get; set; } = new List<UserDocument>();
}
