using IngApp.Domain.Entities.Users;
using IngApp.Domain.Entities.Roles;
using IngApp.Domain.Entities.Suppliers;
using IngApp.Domain.Enums;
using UserTypeEntity = IngApp.Domain.Entities.Users.UserType;

namespace IngApp.Tests.Common;

/// <summary>
/// Helper class برای ساخت Test Data
/// </summary>
public static class TestDataBuilder
{
    public static User CreateUser(
        string phoneNumber = "09123456789",
        string? displayName = null,
        int userTypeId = 1, // Buyer
        bool isActive = true)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            PhoneNumber = phoneNumber,
            DisplayName = displayName ?? "Test User",
            UserTypeId = userTypeId,
            SubscriptionLevel = SubscriptionLevel.None,
            VerificationStatus = VerificationStatus.NotSubmitted,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static UserTypeEntity CreateUserType(
        string code = "Buyer",
        string title = "خریدار",
        bool isActive = true)
    {
        return new UserTypeEntity
        {
            Id = 0, // Will be set by EF
            Code = code,
            Title = title,
            IsActive = isActive
        };
    }

    public static SupplierProfile CreateSupplierProfile(
        Guid userId,
        int supplierTypeId = 1,
        string businessName = "Test Business",
        VerificationStatus status = VerificationStatus.NotSubmitted)
    {
        return new SupplierProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SupplierTypeId = supplierTypeId,
            BusinessName = businessName,
            VerificationStatus = status,
            CreatedAt = DateTime.UtcNow
        };
    }
}

