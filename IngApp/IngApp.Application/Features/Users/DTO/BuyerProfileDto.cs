namespace IngApp.Application.Features.Users.DTO;

public class BuyerProfileDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? UserPhoneNumber { get; set; }
    public string? UserDisplayName { get; set; }
    
    public string? BusinessName { get; set; }
    public string? ContactMobile { get; set; }
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
    
    /// <summary>
    /// شناسه Visitor که این Buyer را معرفی کرده است
    /// </summary>
    public Guid? ReferredByVisitorId { get; set; }
    public string? ReferredByVisitorCode { get; set; }
    public string? ReferredByVisitorName { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}




















