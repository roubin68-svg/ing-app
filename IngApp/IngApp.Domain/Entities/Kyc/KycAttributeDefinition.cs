using System;
using IngApp.Domain.Enums;

namespace IngApp.Domain.Entities.Kyc
{
    /// <summary>
    /// تعریف فیلدهای KYC (کاملاً داینامیک)
    /// مثال: NationalId, BusinessLicense, ImportPermit
    /// </summary>
    public class KycAttributeDefinition
    {
        public int Id { get; set; }

        /// <summary>
        /// عنوان نمایشی برای UI
        /// </summary>
        public string DisplayName { get; set; } = null!;

        public string? Description { get; set; }

        /// <summary>
        /// نوع داده (فایل، متن، عدد، ...)
        /// </summary>
        public KycDataType DataType { get; set; } = KycDataType.File;

        /// <summary>
        /// آیا به صورت پیش‌فرض Required باشد؟
        /// (Template سطح Required را override می‌کند)
        /// </summary>
        public bool DefaultRequired { get; set; } = true;

        /// <summary>
        /// برای مدیریت نمایش/عدم‌نمایش در Template و فرم‌ها.
        /// حذف واقعی نداریم، فقط IsActive را false می‌کنیم.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
