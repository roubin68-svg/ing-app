namespace IngApp.Application.Features.Products.DTO;

public class ProductListQuery
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public int? CategoryId { get; set; }

    public bool? IsActive { get; set; }

    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; }
}
