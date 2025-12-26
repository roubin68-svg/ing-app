using IngApp.Domain.Enums;

namespace IngApp.Application.Features.Products.Attributes;

public class ProductAttributeDefinitionListQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; }

    public string? DisplayName { get; set; }
    public ProductAttributeDataType? DataType { get; set; }
    public bool? IsActive { get; set; }
}
