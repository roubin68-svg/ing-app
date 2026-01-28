using IngApp.Application.Features.Users.DTO;
using IngApp.Domain.Entities.Users;
using IngApp.Infrastructure.Persistence;
using IngApp.Infrastructure.Services.Users;
using IngApp.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IngApp.Tests.Services.Users;

/// <summary>
/// تست‌های UserService
/// </summary>
[Trait("Category", "Unit")]
public class UserServiceTests : TestBase
{
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userService = new UserService(DbContext);
    }

    protected override void SeedDatabase()
    {
        // Seed UserTypes
        DbContext.UserTypes.AddRange(
            TestDataBuilder.CreateUserType("Buyer", "خریدار"),
            TestDataBuilder.CreateUserType("Supplier", "تأمین‌کننده"),
            TestDataBuilder.CreateUserType("Admin", "مدیر سیستم"),
            TestDataBuilder.CreateUserType("Visitor", "بازاریاب")
        );
        DbContext.SaveChanges();
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            PhoneNumber = "09123456789",
            DisplayName = "Test User",
            UserTypeCode = "Buyer",
            IsActive = true
        };

        // Act
        var result = await _userService.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.PhoneNumber.Should().Be("09123456789");
        result.UserTypeCode.Should().Be("Buyer");
        result.UserTypeTitle.Should().Be("خریدار");

        var userInDb = await DbContext.Users
            .Include(u => u.UserType)
            .FirstOrDefaultAsync(u => u.Id == result.Id);
        userInDb.Should().NotBeNull();
        userInDb!.UserType.Code.Should().Be("Buyer");
    }

    [Fact]
    public async Task CreateAsync_WithDuplicatePhoneNumber_ShouldThrowException()
    {
        // Arrange
        var existingUser = TestDataBuilder.CreateUser("09123456789");
        DbContext.Users.Add(existingUser);
        await DbContext.SaveChangesAsync();

        var dto = new CreateUserDto
        {
            PhoneNumber = "09123456789",
            DisplayName = "Another User",
            UserTypeCode = "Buyer"
        };

        // Act & Assert
        await Assert.ThrowsAsync<IngApp.Application.Common.Exceptions.ValidationException>(
            () => _userService.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_WithInvalidUserTypeCode_ShouldThrowException()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            PhoneNumber = "09123456789",
            DisplayName = "Test User",
            UserTypeCode = "InvalidType"
        };

        // Act & Assert
        await Assert.ThrowsAsync<IngApp.Application.Common.Exceptions.ValidationException>(
            () => _userService.CreateAsync(dto));
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnUser()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser("09123456789", "Test User", 1);
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _userService.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.PhoneNumber.Should().Be("09123456789");
        result.DisplayName.Should().Be("Test User");
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _userService.GetByIdAsync(invalidId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPagedAsync_WithUserTypeFilter_ShouldFilterCorrectly()
    {
        // Arrange
        var buyerUser = TestDataBuilder.CreateUser("09111111111", "Buyer User", 1);
        var supplierUser = TestDataBuilder.CreateUser("09222222222", "Supplier User", 2);
        DbContext.Users.AddRange(buyerUser, supplierUser);
        await DbContext.SaveChangesAsync();

        var query = new UserListQueryDto
        {
            Page = 1,
            PageSize = 10,
            UserTypeCode = "Buyer"
        };

        // Act
        var result = await _userService.GetPagedAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().UserTypeCode.Should().Be("Buyer");
    }
}












