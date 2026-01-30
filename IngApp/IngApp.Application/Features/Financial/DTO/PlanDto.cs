namespace IngApp.Application.Features.Financial.DTO;

public class PlanDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int DurationMonths { get; set; }
    public long PriceRial { get; set; }
    public decimal PriceToman => PriceRial / 10m;
    public bool UnlimitedContactViews { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
}




















