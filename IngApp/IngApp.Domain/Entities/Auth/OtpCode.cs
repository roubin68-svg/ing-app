using IngApp.Domain.Enums;

namespace IngApp.Domain.Entities.Auth;

public class OtpCode
{
    public Guid Id { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;

    public string CodeHash { get; set; } = string.Empty;

    public OtpPurpose Purpose { get; set; } = OtpPurpose.Login;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAtUtc { get; set; }

    public bool IsUsed { get; private set; }

    public DateTime? UsedAtUtc { get; private set; }

    public int AttemptCount { get; private set; }

    public DateTime? LastAttemptAtUtc { get; private set; }

    public string? ClientIdentifier { get; set; }

    // ----------- Domain Logic -----------

    public bool IsExpired() => DateTime.UtcNow > ExpiresAtUtc;

    public void RegisterAttempt()
    {
        AttemptCount++;
        LastAttemptAtUtc = DateTime.UtcNow;
    }

    public void MarkAsUsed()
    {
        if (IsUsed) return;

        IsUsed = true;
        UsedAtUtc = DateTime.UtcNow;
    }
}
