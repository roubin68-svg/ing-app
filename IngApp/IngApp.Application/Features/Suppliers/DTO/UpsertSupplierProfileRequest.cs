namespace IngApp.Application.Features.Suppliers.DTO
{
    /// <summary>
    /// ورودی برای ساخت/ویرایش پروفایل Supplier توسط خود کاربر.
    /// </summary>
    public class UpsertSupplierProfileRequest
    {
        public int SupplierTypeId { get; set; }

        public string BusinessName { get; set; } = null!;
        public string? NationalId { get; set; }
        public string? LicenseNumber { get; set; }

        public string? Province { get; set; }
        public string? City { get; set; }
        public string? Address { get; set; }

        public string? ContactName { get; set; }
        public string? ContactPhone { get; set; }
    }
}
