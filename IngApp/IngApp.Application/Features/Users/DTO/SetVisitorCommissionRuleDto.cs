namespace IngApp.Application.Features.Users.DTO;

public class SetVisitorCommissionRuleDto
{
    /// <summary>
    /// کد قانون (UnlockContactCommission, SubscriptionCommission)
    /// </summary>
    public string CommissionRuleCode { get; set; } = null!;
    
    /// <summary>
    /// درصد پورسانت اختصاصی (null = استفاده از پیش‌فرض)
    /// </summary>
    public decimal? CommissionPercentage { get; set; }
    
    public bool IsActive { get; set; } = true;
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}




















