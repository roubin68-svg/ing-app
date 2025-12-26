using IngApp.Application.Features.Auth.DTO;

namespace IngApp.Application.Common.Interfaces.Authentication
{
    public interface IAuthService
    {
        Task<AuthResponse> SendOtpAsync(SendOtpRequest request);
        Task<AuthResponse> VerifyOtpAsync(VerifyOtpRequest request);
        Task<UserInfoResponse> GetUserInfoAsync(Guid userId);
    }
}
