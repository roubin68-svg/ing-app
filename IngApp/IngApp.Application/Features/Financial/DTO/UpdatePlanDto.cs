namespace IngApp.Application.Features.Financial.DTO;

public class UpdatePlanDto
{
    public string Code { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int DurationMonths { get; set; }
    public long PriceRial { get; set; }
    public bool UnlimitedContactViews { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;
}




















