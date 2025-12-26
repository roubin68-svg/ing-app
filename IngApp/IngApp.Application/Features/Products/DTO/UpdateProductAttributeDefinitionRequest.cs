using IngApp.Domain.Enums;

namespace IngApp.Application.Features.Products.DTO;

public class UpdateProductAttributeDefinitionRequest
{
    public string DisplayName { get; set; } = null!;
    public ProductAttributeDataType DataType { get; set; }
    public string? Unit { get; set; }
}
