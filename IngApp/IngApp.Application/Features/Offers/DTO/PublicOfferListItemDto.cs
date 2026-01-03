namespace IngApp.Application.Features.Offers.DTO;

public class PublicOfferListItemDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public int ProductCategoryId { get; set; }
    public string ProductCategoryName { get; set; } = null!;

    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = null!;

    public DateTime PublishedAt { get; set; }
    public DateTime SearchDateTime { get; set; }
}
