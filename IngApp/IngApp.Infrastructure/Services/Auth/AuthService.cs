using IngApp.Application.Common.Exceptions;
using IngApp.Application.Common.Interfaces.Authentication;
using IngApp.Application.Features.Auth.DTO;
using IngApp.Application.Features.Users.DTO;
using IngApp.Domain.Entities.Users;
using IngApp.Domain.Enums;
using IngApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using IngApp.Domain.Entities;
using IngApp.Infrastructure.Persistence.Configurations;


namespace IngApp.Infrastructure.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IOtpService _otpService;
    private readonly AppDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        IOtpService otpService,
        AppDbContext context,
        IJwtTokenService jwtTokenService)
    {
        _otpService = otpService;
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    // ============================
    // SEND OTP
    // ============================
    public async Task<AuthResponse> SendOtpAsync(SendOtpRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            throw new ValidationException(new() { "شماره موبایل اجباری است." });

        var phone = request.PhoneNumber.Trim();

        var existingUser = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.PhoneNumber == phone);

        if (existingUser != null && !existingUser.IsActive)
            throw new AppException("حساب کاربری شما توسط مدیر سیستم غیرفعال شده است!");

        await _otpService.GenerateCodeAsync(phone);

        // SendOtp هیچ داده‌ای لازم ندارد
        return new AuthResponse();
    }

    // ============================
    // VERIFY OTP
    // ============================
    public async Task<AuthResponse> VerifyOtpAsync(VerifyOtpRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            throw new ValidationException(new() { "شماره موبایل اجباری است." });

        if (string.IsNullOrWhiteSpace(request.Code))
            throw new ValidationException(new() { "کد تأیید اجباری است." });

        var (ok, errorMsg) = await _otpService.ValidateCodeAsync(request.PhoneNumber, request.Code);
        if (!ok)
            throw new ValidationException(new() { errorMsg });

        var phone = request.PhoneNumber.Trim();

        var user = await _context.Users
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                    .ThenInclude(x => x.RolePermissions)
                        .ThenInclude(x => x.Permission)
            .FirstOrDefaultAsync(x => x.PhoneNumber == phone);

        if (user != null && !user.IsActive)
            throw new AppException("حساب کاربری شما غیرفعال شده است.");

        if (user == null)
        {
            user = new User
            {
                PhoneNumber = phone,
                UserType = UserType.Buyer,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        // تضمین Role پیش‌فرض Buyer برای هر کاربر
        var buyerRoleId = RoleConfiguration.BuyerRoleId;

        var hasBuyerRole = await _context.UserRoles
            .AnyAsync(x => x.UserId == user.Id && x.RoleId == buyerRoleId);

        if (!hasBuyerRole)
        {
            _context.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = buyerRoleId
            });

            await _context.SaveChangesAsync();
        }

        // چون برای user جدید navigationها لود نیست، دوباره با Include کامل لود می‌کنیم
        user = await _context.Users
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                    .ThenInclude(x => x.RolePermissions)
                        .ThenInclude(x => x.Permission)
            .FirstOrDefaultAsync(x => x.Id == user.Id);



        var roles = user.UserRoles.Select(r => r.Role.Name).Distinct().ToList();

        var permissions = user.UserRoles
            .SelectMany(r => r.Role.RolePermissions)
            .Select(p => p.Permission.Code)
            .Distinct()
            .ToList();

        var (token, expiration) = _jwtTokenService.GenerateToken(user, permissions);

        return new AuthResponse
        {
            Token = token,
            Expiration = expiration,
            Roles = roles,
            Permissions = permissions
        };
    }


    // ============================================================
    // GET USER INFO
    // ============================================================
    public async Task<UserInfoResponse> GetUserInfoAsync(Guid userId)
    {
        var user = await _context.Users
                            .Include(x => x.UserRoles)
                            .ThenInclude(x => x.Role)
                            .ThenInclude(x => x.RolePermissions)
                            .ThenInclude(x => x.Permission)
                            .FirstOrDefaultAsync(x => x.Id == userId);


        if (user == null)
            throw new NotFoundException("کاربر یافت نشد.");

        var roles = user.UserRoles.Select(r => r.Role.Name).ToList();

        var permissions = user.UserRoles
            .SelectMany(r => r.Role.RolePermissions)
            .Select(p => p.Permission.Code)
            .Distinct()
            .ToList();

        return new UserInfoResponse
        {
            Id = user.Id,
            PhoneNumber = user.PhoneNumber,
            DisplayName = user.DisplayName ?? "",
            UserType = user.UserType.ToString(),
            IsActive = user.IsActive,
            SubscriptionLevel = (int)user.SubscriptionLevel,
            VerificationStatus = (int)user.VerificationStatus,
            Roles = roles,
            Permissions = permissions
        };
    }
}
