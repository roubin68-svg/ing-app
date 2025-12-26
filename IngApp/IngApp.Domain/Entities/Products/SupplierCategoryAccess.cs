using System;

namespace IngApp.Domain.Entities.Products;

public class SupplierCategoryAccess
{
    public int Id { get; set; }

    public Guid UserId { get; set; }

    public int ProductCategoryId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation
    public ProductCategory ProductCategory { get; set; } = null!;
}
