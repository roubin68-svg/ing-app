namespace IngApp.Application.Features.Users.DTO;

/// <summary>
/// DTO برای نمایش Visitor در صفحه مدیریت Admin
/// </summary>
public class VisitorManagementDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserPhoneNumber { get; set; } = null!;
    public string? UserDisplayName { get; set; }
    public string ReferralCode { get; set; } = null!;
    public string? BusinessName { get; set; }
    public string? ContactMobile { get; set; }
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int BuyerCount { get; set; } // تعداد Buyer های معرفی شده
    public long TotalCommissionRial { get; set; } // مجموع پورسانت دریافتی
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}




















