using IngApp.Domain.Entities.Kyc;

public class KycTemplate
{
    public int Id { get; set; }

    public int SupplierTypeId { get; set; }

    public int KycAttributeDefinitionId { get; set; }

    // ✅ Navigation Property (علت اصلی Error)
    public KycAttributeDefinition KycAttributeDefinition { get; set; }

    public bool IsRequired { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
