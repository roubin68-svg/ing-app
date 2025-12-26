namespace IngApp.Application.Features.Offers.Requests;

public class SaveOfferDocumentsRequest
{
    public List<OfferDocumentItem> Items { get; set; } = [];
}

public class OfferDocumentItem
{
    public int AttributeDefinitionId { get; set; }
    public string? Value { get; set; }
    public string? FilePath { get; set; }
}
