namespace IngApp.Application.Features.Users.DTO;

/// <summary>
/// DTO برای نمایش Buyer در صفحه مدیریت Admin
/// </summary>
public class BuyerManagementDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserPhoneNumber { get; set; } = null!;
    public string? UserDisplayName { get; set; }
    
    public string? BusinessName { get; set; }
    public string? ContactMobile { get; set; }
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
    
    // اطلاعات بازاریاب
    public Guid? ReferredByVisitorId { get; set; }
    public string? ReferredByVisitorCode { get; set; }
    public string? ReferredByVisitorName { get; set; }
    public string? ReferredByVisitorPhoneNumber { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}



