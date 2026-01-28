namespace IngApp.Domain.Entities.Users;

/// <summary>
/// نوع کاربر (Buyer, Supplier, Admin, Visitor)
/// کاملاً داینامیک و قابل مدیریت توسط Admin.
/// </summary>
public class UserType
{
    public int Id { get; set; }

    /// <summary>
    /// کد یکتا برای شناسایی نوع کاربر (Admin, Supplier, Buyer, Visitor)
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// عنوان قابل نمایش در UI (فارسی)
    /// </summary>
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}












