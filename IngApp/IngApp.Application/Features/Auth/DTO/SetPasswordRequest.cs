namespace IngApp.Application.Features.Auth.DTO;

public class SetPasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty; // برای تغییر Password
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}



