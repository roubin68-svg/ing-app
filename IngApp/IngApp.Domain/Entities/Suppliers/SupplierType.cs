using System;

namespace IngApp.Domain.Entities.Suppliers
{
    /// <summary>
    /// نوع تأمین‌کننده (واردکننده، تولیدکننده، کشاورز و ...)
    /// کاملاً داینامیک و قابل مدیریت توسط Admin.
    /// </summary>
    public class SupplierType
    {
        public int Id { get; set; }


        /// <summary>
        /// نام قابل نمایش در UI (فارسی)
        /// </summary>
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
