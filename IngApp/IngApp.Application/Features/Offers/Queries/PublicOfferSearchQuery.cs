namespace IngApp.Application.Features.Offers.Queries;

public class PublicOfferSearchQuery
{
    public int? OfferId { get; set; }
    public int? ProductId { get; set; }
    public int? CategoryId { get; set; }

    public string? ProductName { get; set; }

    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    public string? SortBy { get; set; } // "newest", "oldest", "priceAsc", "priceDesc", "quantityAsc", "quantityDesc"
    public string? SortDir { get; set; } // "asc", "desc"

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
