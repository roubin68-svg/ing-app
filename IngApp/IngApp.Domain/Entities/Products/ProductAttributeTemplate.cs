namespace IngApp.Domain.Entities.Products;

public class ProductAttributeTemplate
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int AttributeDefinitionId { get; set; }
    public ProductAttributeDefinition AttributeDefinition { get; set; } = null!;

    public bool IsRequired { get; set; }
}
