using IngApp.Domain.Enums;

namespace IngApp.Application.Features.Offers.Queries;

public class MyOffersQuery
{
    // Filters
    public OfferStatus? Status { get; set; }
    public int? ProductCategoryId { get; set; }
    public string? ProductName { get; set; }

    // Paging
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    // Sorting
    public string? SortBy { get; set; }        // createdAt | productName
    public string? SortDirection { get; set; } // asc | desc
}
