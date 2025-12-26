namespace IngApp.Application.Features.Offers.DTO;

public class PublicOfferListItemDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public decimal TotalPrice { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = null!;

    public DateTime PublishedAt { get; set; }
}
