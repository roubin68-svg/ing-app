using IngApp.Application.Features.Suppliers.DTO;
using IngApp.Domain.Entities.Users;
using IngApp.Domain.Entities.Suppliers;
using IngApp.Domain.Enums;
using IngApp.Infrastructure.Persistence;
using IngApp.Infrastructure.Services.Suppliers;
using IngApp.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IngApp.Tests.Services.Suppliers;

/// <summary>
/// تست‌های SupplierProfileService
/// </summary>
[Trait("Category", "Unit")]
public class SupplierProfileServiceTests : TestBase
{
    private readonly SupplierProfileService _supplierProfileService;

    public SupplierProfileServiceTests()
    {
        _supplierProfileService = new SupplierProfileService(DbContext);
    }

    protected override void SeedDatabase()
    {
        // Seed UserTypes
        DbContext.UserTypes.AddRange(
            TestDataBuilder.CreateUserType("Buyer", "خریدار"),
            TestDataBuilder.CreateUserType("Supplier", "تأمین‌کننده")
        );

        // Seed SupplierTypes
        DbContext.SupplierTypes.Add(new SupplierType
        {
            Id = 1,
            Name = "تولیدکننده",
            IsActive = true
        });

        DbContext.SaveChanges();
    }

    [Fact]
    public async Task UpsertForUserAsync_WithValidData_ShouldCreateProfile()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser("09123456789", "Test User", 1);
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        var request = new UpsertSupplierProfileRequest
        {
            SupplierTypeId = 1,
            BusinessName = "Test Business",
            NationalId = "1234567890",
            LicenseNumber = "LIC123",
            BusinessType = BusinessType.Legal,
            ContactName = "John Doe",
            ContactPosition = ContactPosition.CEO,
            ContactMobile = "09123456789"
        };

        // Act
        var result = await _supplierProfileService.UpsertForUserAsync(user.Id, request);

        // Assert
        result.Should().NotBeNull();
        result.BusinessName.Should().Be("Test Business");
        result.BusinessType.Should().Be(BusinessType.Legal);

        var profileInDb = await DbContext.SupplierProfiles
            .FirstOrDefaultAsync(p => p.UserId == user.Id);
        profileInDb.Should().NotBeNull();
    }

    [Fact]
    public async Task SubmitForUserAsync_WithDraftProfile_ShouldChangeStatusToPending()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser("09123456789", "Test User", 1);
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        var profile = TestDataBuilder.CreateSupplierProfile(
            user.Id,
            supplierTypeId: 1,
            businessName: "Test Business",
            status: VerificationStatus.NotSubmitted);
        DbContext.SupplierProfiles.Add(profile);
        await DbContext.SaveChangesAsync();

        // Act
        await _supplierProfileService.SubmitForUserAsync(user.Id);

        // Assert
        var profileInDb = await DbContext.SupplierProfiles
            .FirstOrDefaultAsync(p => p.UserId == user.Id);
        profileInDb.Should().NotBeNull();
        profileInDb!.VerificationStatus.Should().Be(VerificationStatus.Pending);
    }
}












