using IngApp.Domain.Enums;

namespace IngApp.Application.Features.Offers.DTO;

public class OfferDocumentDto
{
    public int AttributeDefinitionId { get; set; }
    public string DisplayName { get; set; } = null!;
    public ProductAttributeDataType DataType { get; set; }

    public string? Value { get; set; }
    public string? FilePath { get; set; }
}
