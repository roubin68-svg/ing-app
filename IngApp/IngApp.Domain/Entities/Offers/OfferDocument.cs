namespace IngApp.Domain.Entities.Offers;

public class OfferDocument
{
    public int Id { get; set; }

    public int OfferId { get; set; }

    public int AttributeDefinitionId { get; set; }

    // --------------------
    // Value
    // --------------------
    public string? Value { get; set; }       // متن، عدد، یا نام فایل
    public string? FilePath { get; set; }    // فقط برای File
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }


    // --------------------
    // Navigation
    // --------------------
    public Offer Offer { get; set; } = null!;
}
