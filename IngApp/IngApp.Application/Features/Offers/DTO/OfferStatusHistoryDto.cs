using IngApp.Domain.Enums;

namespace IngApp.Application.Features.Offers.DTO;

public class OfferStatusHistoryDto
{
    public int Id { get; set; }

    public int OfferId { get; set; }

    public OfferStatus OldStatus { get; set; }
    public OfferStatus NewStatus { get; set; }

    public string? AdminUserId { get; set; }
    public string? AdminDisplayName { get; set; }
    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }
}





















