public class UserInfoResponse
{
    public Guid Id { get; set; }
    public string PhoneNumber { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string UserType { get; set; } = "";

    public bool IsActive { get; set; }
    public int SubscriptionLevel { get; set; }
    public int VerificationStatus { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
}
