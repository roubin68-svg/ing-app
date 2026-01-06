using System.Collections.Generic;

namespace IngApp.Domain.Entities.Products;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Unit { get; set; }

    public int CategoryId { get; set; }
    public ProductCategory Category { get; set; } = null!;

    public string? ImagePath { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<ProductAttributeTemplate> AttributeTemplates { get; set; }
        = new List<ProductAttributeTemplate>();
}
