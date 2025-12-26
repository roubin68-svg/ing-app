using IngApp.Domain.Enums;

namespace IngApp.Domain.Entities.Products;

public class ProductAttributeDefinition
{
    public int Id { get; set; }

    public string DisplayName { get; set; } = string.Empty;
    public ProductAttributeDataType DataType { get; set; }
    public string? Unit { get; set; }

    public bool IsActive { get; set; } = true;
}
