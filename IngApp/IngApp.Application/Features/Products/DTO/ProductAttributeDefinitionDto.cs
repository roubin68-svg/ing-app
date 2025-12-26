using IngApp.Domain.Enums;

namespace IngApp.Application.Features.Products.DTO;

public class ProductAttributeDefinitionDto
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = null!;
    public ProductAttributeDataType DataType { get; set; }
    public string? Unit { get; set; }
    public bool IsActive { get; set; }
}
