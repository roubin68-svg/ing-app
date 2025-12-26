
namespace IngApp.Application.Features.Products.DTO;
public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public string? Unit { get; set; } = string.Empty;
}
