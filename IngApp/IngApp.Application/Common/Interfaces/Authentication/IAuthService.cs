using IngApp.Application.Features.Auth.DTO;

namespace IngApp.Application.Common.Interfaces.Authentication
{
    public interface IAuthService
    {
        Task<AuthResponse> SendOtpAsync(SendOtpRequest request);
        Task<AuthResponse> VerifyOtpAsync(VerifyOtpRequest request);
        Task<AuthResponse> LoginWithPasswordAsync(LoginWithPasswordRequest request);
        Task<UserInfoResponse> GetUserInfoAsync(Guid userId);
        Task UpdateMyProfileAsync(Guid userId, UpdateMyProfileRequest request);
        Task SetPasswordAsync(Guid userId, SetPasswordRequest request);
    }
}
