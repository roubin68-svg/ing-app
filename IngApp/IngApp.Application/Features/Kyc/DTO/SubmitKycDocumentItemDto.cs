using IngApp.Domain.Entities.Kyc;
using IngApp.Domain.Enums;

namespace IngApp.Application.Features.Kyc.DTO

{
    /// <summary>
    /// یک آیتم از مدارک ارسالی KYC توسط کاربر.
    /// در API، FilePath بعد از آپلود فایل ست می‌شود.
    /// </summary>
    public class SubmitKycDocumentItemDto
    {
        public int AttributeDefinitionId { get; set; }

        public KycDataType DataType { get; set; }

        /// <summary>
        /// برای Text/Number/Boolean/Enum استفاده می‌شود.
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// برای File استفاده می‌شود (مسیر فایل ذخیره‌شده).
        /// </summary>
        public string? FilePath { get; set; }
    }
}
