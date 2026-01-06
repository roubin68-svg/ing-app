using IngApp.Domain.Enums;

namespace IngApp.Application.Features.Offers.DTO;

public class OfferHeaderDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? ProductImagePath { get; set; }

    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = null!;

    public bool HasTax { get; set; }
    public decimal? TaxAmount { get; set; }

    public OfferStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ExpireAtBySupplier { get; set; }

    public OfferWizardStep WizardStep { get; set; }

    public Guid SupplierUserId { get; set; }

    public string? RejectedReason { get; set; }

    // Additional fields for Admin view
    public int? ProductCategoryId { get; set; }
    public string? ProductCategoryName { get; set; }
    public string? SupplierBusinessName { get; set; }
}
