using IngApp.Domain.Enums;

namespace IngApp.Domain.Entities.Offers;

public class OfferStatusHistory
{
    public int Id { get; set; }

    public int OfferId { get; set; }
    public Offer Offer { get; set; } = null!;

    public OfferStatus OldStatus { get; set; }
    public OfferStatus NewStatus { get; set; }

    public string? AdminUserId { get; set; } // از Claim خوانده می‌شود
    public string? Note { get; set; } // دلیل رد یا یادداشت

    public DateTime CreatedAt { get; set; }
}

