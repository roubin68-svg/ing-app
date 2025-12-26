
namespace IngApp.Application.Features.Products.DTO;

public class ProductDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public string? Unit { get; set; }

    public bool IsActive { get; set; }

    public string CategoryName { get; set; } = string.Empty;
}
