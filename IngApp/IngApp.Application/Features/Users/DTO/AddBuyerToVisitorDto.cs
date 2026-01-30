namespace IngApp.Application.Features.Users.DTO;

public class AddBuyerToVisitorDto
{
    /// <summary>
    /// شماره موبایل Buyer
    /// </summary>
    public string Mobile { get; set; } = null!;
    
    /// <summary>
    /// نام Buyer (فقط در صورتی که User وجود نداشته باشد)
    /// </summary>
    public string? BuyerName { get; set; }
}




















