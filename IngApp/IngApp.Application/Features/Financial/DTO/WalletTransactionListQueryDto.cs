namespace IngApp.Application.Features.Financial.DTO;

/// <summary>
/// پارامترهای جستجوی گزارش تراکنش‌های کیف پول (دفتر کل)
/// </summary>
public class WalletTransactionListQueryDto
{
    /// <summary>
    /// صفحه فعلی
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// تعداد در هر صفحه
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// فیلتر بر اساس شناسه کاربر
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// فیلتر بر اساس شماره موبایل کاربر
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// فیلتر بر اساس نام/عنوان کاربر
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// کد جهت تراکنش (Credit / Debit)
    /// </summary>
    public string? DirectionCode { get; set; }

    /// <summary>
    /// کد نوع عملیات مالی (ManualDeposit, ManualWithdrawal, UnlockContactFee, ...)
    /// </summary>
    public string? OperationTypeCode { get; set; }

    /// <summary>
    /// کد وضعیت تراکنش (Pending, Committed, Failed, Reversed)
    /// </summary>
    public string? StatusCode { get; set; }

    /// <summary>
    /// کد نوع مرجع تراکنش (Offer, Subscription, AdminAction, ...)
    /// </summary>
    public string? ReferenceTypeCode { get; set; }

    /// <summary>
    /// دسته منبع تراکنش برای گزارش:
    /// Bank = تراکنش‌های واقعی بانکی (TopUp/Payment)
    /// Commission = تراکنش‌های مربوط به پورسانت (CommissionEarned و پرداخت پورسانت در آینده)
    /// Manual = عملیات دستی مدیر (ManualDeposit/ManualWithdrawal با AdminAction)
    /// Other = سایر موارد
    /// </summary>
    public string? SourceCategory { get; set; }

    /// <summary>
    /// حداقل تاریخ ایجاد تراکنش (میلادی) - از سمت UI از تاریخ شمسی تبدیل می‌شود
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// حداکثر تاریخ ایجاد تراکنش (میلادی، انتهای روز)
    /// </summary>
    public DateTime? ToDate { get; set; }
}


