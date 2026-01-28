namespace IngApp.Application.Features.Users.DTO;

public class UpsertBuyerProfileDto
{
    public string? BusinessName { get; set; }
    public string? ContactMobile { get; set; }
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
    
    /// <summary>
    /// کد معرف (ReferralCode) Visitor که این Buyer را معرفی کرده است
    /// </summary>
    public string? ReferrerVisitorCode { get; set; }
}











