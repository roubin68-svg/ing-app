using IngApp.Domain.Enums;

namespace IngApp.Application.Features.Offers.Queries;

public class AdminOffersQuery
{
    public int? OfferId { get; set; }
    
    // Filters
    public OfferStatus? Status { get; set; }
    public Guid? SupplierUserId { get; set; }
    public int? ProductCategoryId { get; set; }
    public string? ProductName { get; set; }

    // Paging
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    // Sorting
    public string? SortBy { get; set; }        // createdAt | productName | status
    public string? SortDirection { get; set; } // asc | desc
}












