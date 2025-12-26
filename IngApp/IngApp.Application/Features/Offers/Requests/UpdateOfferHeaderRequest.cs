namespace IngApp.Application.Features.Offers.Requests;

public class UpdateOfferHeaderRequest
{
    public decimal UnitPrice { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = null!;

    public bool HasTax { get; set; }
    public decimal? TaxAmount { get; set; }

    public DateTime? ExpireAtBySupplier { get; set; }
}
