namespace IngApp.Application.Features.Products.DTO;
public class CreateProductCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public int? ParentId { get; set; }
}