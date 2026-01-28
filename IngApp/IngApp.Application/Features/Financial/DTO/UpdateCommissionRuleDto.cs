using System.ComponentModel.DataAnnotations;

namespace IngApp.Application.Features.Financial.DTO;

/// <summary>
/// DTO برای به‌روزرسانی قانون پورسانت
/// </summary>
public class UpdateCommissionRuleDto
{
    [Required(ErrorMessage = "عنوان الزامی است")]
    [MaxLength(200, ErrorMessage = "عنوان نمی‌تواند بیشتر از 200 کاراکتر باشد")]
    public string Title { get; set; } = null!;

    [MaxLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 1000 کاراکتر باشد")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "درصد پورسانت الزامی است")]
    [Range(0, 100, ErrorMessage = "درصد پورسانت باید بین 0 تا 100 باشد")]
    public decimal CommissionPercentage { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}


