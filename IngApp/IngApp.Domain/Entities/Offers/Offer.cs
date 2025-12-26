using IngApp.Domain.Enums;

namespace IngApp.Domain.Entities.Offers;

public class Offer
{
    public int Id { get; set; }

    // --------------------
    // Ownership & Identity
    // --------------------
    public Guid SupplierUserId { get; set; }

    public int ProductId { get; set; }

    public OfferWizardStep WizardStep { get; set; }


    // --------------------
    // Commercial Fields
    // --------------------
    public decimal UnitPrice { get; set; }      // قیمت واحد
    public decimal TotalPrice { get; set; }     // قیمت کل

    public decimal Quantity { get; set; }       // مقدار (مثلاً 30000)
    public string Unit { get; set; } = null!;   // kg, liter, ...

    public bool HasTax { get; set; }
    public decimal? TaxAmount { get; set; }

    // --------------------
    // Lifecycle
    // --------------------
    public OfferStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public DateTime? PublishedAt { get; set; }

    public DateTime? ExpireAtBySupplier { get; set; }
    public DateTime? ExpireAtBySystem { get; set; }

    // برای Sort و Search (نردبان)
    public DateTime SearchDateTime { get; set; }

    // --------------------
    // Management / Reason
    // --------------------
    public string? CancelReason { get; set; }
    public string? RejectedReason { get; set; }

    // --------------------
    // Navigation
    // --------------------
    public ICollection<OfferDocument> Documents { get; set; } = new List<OfferDocument>();
}
