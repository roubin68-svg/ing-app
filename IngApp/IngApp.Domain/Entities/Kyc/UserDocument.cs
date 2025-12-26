using System;
using IngApp.Domain.Enums;
using IngApp.Domain.Entities.Users;

namespace IngApp.Domain.Entities.Kyc
{
    /// <summary>
    /// مدارک آپلود شده توسط کاربر (KYC Dynamic)
    /// </summary>
    public class UserDocument
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        // Navigation
        public User User { get; set; } = null!;

        public int KycAttributeDefinitionId { get; set; }

        /// <summary>
        /// مقدار فیلد (برای Text/Number/...)
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// فایل آپلود شده (اگر DataType=File)
        /// </summary>
        public string? FilePath { get; set; }

        /// <summary>
        /// Soft delete برای زمانی که SupplierType تغییر می‌کند یا مدارک باید بی‌اعتبار شوند.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        public DocumentStatus Status { get; set; } = DocumentStatus.Pending;
        public string? AdminNote { get; set; }

        public DateTime UploadedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}
