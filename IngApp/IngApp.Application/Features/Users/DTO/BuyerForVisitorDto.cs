namespace IngApp.Application.Features.Users.DTO;

public class BuyerForVisitorDto
{
    public Guid BuyerProfileId { get; set; }
    public Guid UserId { get; set; }
    public string UserPhoneNumber { get; set; } = null!;
    public string? UserDisplayName { get; set; }
    public string? BusinessName { get; set; }
    public DateTime ReferredAt { get; set; } // تاریخ معرفی (CreatedAt از BuyerProfile)
}




















