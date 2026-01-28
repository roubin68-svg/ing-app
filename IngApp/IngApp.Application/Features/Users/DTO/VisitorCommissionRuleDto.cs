namespace IngApp.Application.Features.Users.DTO;

public class VisitorCommissionRuleDto
{
    public int Id { get; set; }
    public string CommissionRuleCode { get; set; } = null!;
    public string CommissionRuleTitle { get; set; } = null!;
    public decimal? CommissionPercentage { get; set; } // null = استفاده از پیش‌فرض
    public decimal DefaultCommissionPercentage { get; set; } // درصد پیش‌فرض از CommissionRule
    public bool IsActive { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}











