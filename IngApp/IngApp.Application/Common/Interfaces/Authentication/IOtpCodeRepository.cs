using IngApp.Domain.Entities.Auth;
using IngApp.Domain.Enums;

namespace IngApp.Application.Common.Interfaces.Authentication;

public interface IOtpCodeRepository
{
    Task<OtpCode?> GetLatestActiveOtpAsync(string phoneNumber, OtpPurpose purpose);
    Task AddAsync(OtpCode otp);
    Task SaveChangesAsync();
}
