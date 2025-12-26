using System.Threading.Tasks;

namespace IngApp.Application.Common.Interfaces.Authentication;

public interface IOtpService
{
    /// <summary>
    /// Generate OTP code, store it and send via SMS.
    /// Returns raw code only for logging/debug.
    /// </summary>
    Task<string> GenerateCodeAsync(string phoneNumber);

    /// <summary>
    /// Validate OTP code and return success flag + message.
    /// </summary>
    Task<(bool Success, string Message)> ValidateCodeAsync(string phoneNumber, string code);
}
