namespace IngApp.Application.Features.Financial.DTO;

/// <summary>
/// DTO برای نمایش قانون پورسانت
/// </summary>
public class CommissionRuleDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal CommissionPercentage { get; set; }
    public bool IsActive { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}


