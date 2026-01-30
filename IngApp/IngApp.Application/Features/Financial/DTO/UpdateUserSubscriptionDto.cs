namespace IngApp.Application.Features.Financial.DTO;

/// <summary>
/// DTO برای ویرایش اشتراک کاربر
/// </summary>
public class UpdateUserSubscriptionDto
{
    /// <summary>
    /// تاریخ شروع اشتراک
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// تاریخ پایان اشتراک
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// کد وضعیت اشتراک (Active, Expired, Cancelled, Pending)
    /// </summary>
    public string? StatusCode { get; set; }

    /// <summary>
    /// تاریخ لغو (اگر اشتراک لغو شده باشد)
    /// </summary>
    public DateTime? CancelledAt { get; set; }
}



