using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IngApp.Domain.Entities.Financial;
using IngApp.Domain.Entities.Kyc;
using IngApp.Domain.Enums;

namespace IngApp.Domain.Entities.Users;

public class User
{
    public Guid Id { get; set; }

    public string PhoneNumber { get; set; } = string.Empty; // یکتا، برای OTP Login
    public string? DisplayName { get; set; }                // نام کسب‌وکار / نام کاربری
    public string? PasswordHash { get; set; }               // Hash رمز عبور (برای Login با Password)

    public int UserTypeId { get; set; }
    public UserType UserType { get; set; } = null!;
    
    public SubscriptionLevel SubscriptionLevel { get; set; } = SubscriptionLevel.None;

    public bool IsActive { get; set; } = true;

    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.NotSubmitted;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }

    // ناوبری‌ها
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<UserDocument> Documents { get; set; } = new List<UserDocument>();
    public Wallet? Wallet { get; set; }
    public ICollection<Financial.UserSubscription> UserSubscriptions { get; set; } = new List<Financial.UserSubscription>();
    public VisitorProfile? VisitorProfile { get; set; }
    public BuyerProfile? BuyerProfile { get; set; }
}
