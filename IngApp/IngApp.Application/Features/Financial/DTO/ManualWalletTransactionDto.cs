using System.ComponentModel.DataAnnotations;

namespace IngApp.Application.Features.Financial.DTO;

/// <summary>
/// DTO برای واریز/برداشت دستی
/// </summary>
public class ManualWalletTransactionDto
{
    [Required(ErrorMessage = "مبلغ الزامی است")]
    [Range(1, long.MaxValue, ErrorMessage = "مبلغ باید بیشتر از صفر باشد")]
    public long AmountRial { get; set; }

    [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد")]
    public string? Description { get; set; }
}

