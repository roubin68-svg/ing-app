using System.Collections.Generic;

namespace IngApp.Domain.Entities.Products;

public class ProductCategory
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public int? ParentId { get; set; }
    public ProductCategory? Parent { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<ProductCategory> Children { get; set; } = new List<ProductCategory>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
