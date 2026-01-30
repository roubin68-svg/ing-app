namespace IngApp.Application.Features.Financial.DTO;

public class UserSubscriptionListQueryDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? UserId { get; set; }
    public string? StatusCode { get; set; } // Active, Expired, Cancelled, Pending
    public int? PlanId { get; set; }
    public string? UserPhoneNumber { get; set; }
    public string? UserDisplayName { get; set; }
}




















