using IngApp.Domain.Enums;

namespace IngApp.Application.Features.Offers.DTO;

public class OfferListItemDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;

    public int ProductCategoryId { get; set; }
    public string ProductCategoryName { get; set; } = null!;

    public decimal TotalPrice { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = null!;

    public OfferStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}
