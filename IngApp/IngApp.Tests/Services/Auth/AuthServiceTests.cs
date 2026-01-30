using IngApp.Application.Features.Auth.DTO;
using IngApp.Domain.Entities.Users;
using IngApp.Infrastructure.Persistence;
using IngApp.Infrastructure.Services.Auth;
using IngApp.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Moq;
using IngApp.Application.Common.Interfaces.Authentication;
using Xunit;

namespace IngApp.Tests.Services.Auth;

/// <summary>
/// تست‌های AuthService
/// </summary>
[Trait("Category", "Unit")]
public class AuthServiceTests : TestBase
{
    private readonly AuthService _authService;
    private readonly Mock<IOtpService> _otpServiceMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;

    public AuthServiceTests()
    {
        _otpServiceMock = new Mock<IOtpService>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _authService = new AuthService(_otpServiceMock.Object, DbContext, _jwtTokenServiceMock.Object);
    }

    protected override void SeedDatabase()
    {
        // Seed UserTypes
        DbContext.UserTypes.AddRange(
            TestDataBuilder.CreateUserType("Buyer", "خریدار"),
            TestDataBuilder.CreateUserType("Supplier", "تأمین‌کننده"),
            TestDataBuilder.CreateUserType("Admin", "مدیر سیستم")
        );

        // Seed Roles (برای جلوگیری از NullReferenceException)
        var buyerRole = new IngApp.Domain.Entities.Roles.Role
        {
            Id = Guid.NewGuid(),
            Name = "Buyer",
            IsActive = true
        };
        DbContext.Roles.Add(buyerRole);
        DbContext.SaveChanges();
    }

    [Fact]
    public async Task VerifyOtpAsync_WithNewPhoneNumber_ShouldCreateUser()
    {
        // Arrange
        var request = new VerifyOtpRequest
        {
            PhoneNumber = "09123456789",
            Code = "123456"
        };

        _otpServiceMock
            .Setup(x => x.ValidateCodeAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((true, "کد معتبر است"));

        _jwtTokenServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()))
            .Returns(("test-token", DateTime.Now.AddDays(7)));

        // Act
        var result = await _authService.VerifyOtpAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("test-token");

        // Reload user with includes
        var userInDb = await DbContext.Users
            .Include(u => u.UserType)
            .FirstOrDefaultAsync(u => u.PhoneNumber == "09123456789");
        userInDb.Should().NotBeNull();
        userInDb!.UserType.Code.Should().Be("Buyer"); // Default UserType
    }

    [Fact]
    public async Task VerifyOtpAsync_WithExistingUser_ShouldReturnToken()
    {
        // Arrange
        var buyerRole = await DbContext.Roles.FirstAsync();
        var existingUser = TestDataBuilder.CreateUser("09123456789", "Existing User", 1);
        var userRole = new IngApp.Domain.Entities.Users.UserRole
        {
            UserId = existingUser.Id,
            RoleId = buyerRole.Id,
            Role = buyerRole
        };
        existingUser.UserRoles.Add(userRole);
        DbContext.Users.Add(existingUser);
        await DbContext.SaveChangesAsync();

        var request = new VerifyOtpRequest
        {
            PhoneNumber = "09123456789",
            Code = "123456"
        };

        _otpServiceMock
            .Setup(x => x.ValidateCodeAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((true, "کد معتبر است"));

        _jwtTokenServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()))
            .Returns(("test-token", DateTime.Now.AddDays(7)));

        // Act
        var result = await _authService.VerifyOtpAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("test-token");
    }

    [Fact]
    public async Task VerifyOtpAsync_WithInvalidOtp_ShouldThrowException()
    {
        // Arrange
        var request = new VerifyOtpRequest
        {
            PhoneNumber = "09123456789",
            Code = "123456"
        };

        _otpServiceMock
            .Setup(x => x.ValidateCodeAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((false, "کد نامعتبر است"));

        // Act & Assert
        await Assert.ThrowsAsync<IngApp.Application.Common.Exceptions.ValidationException>(
            () => _authService.VerifyOtpAsync(request));
    }
}

