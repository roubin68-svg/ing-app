using IngApp.Domain.Entities.Kyc;
using IngApp.Domain.Enums;

namespace IngApp.Application.Features.Kyc.DTO
{
    /// <summary>
    /// چیزی که Front برای ساخت فرم KYC لازم دارد:
    /// تعریف فیلد + Required بودن + وضعیت فعلی (در صورت وجود مدرک).
    /// </summary>
    public class KycRequirementDto
    {
        public int AttributeDefinitionId { get; set; }
        public string AttributeDisplayName { get; set; } = null!;
        public string? Description { get; set; }
        public KycDataType DataType { get; set; }
        public bool IsRequired { get; set; }

        // وضعیت فعلی مدرک (اگر قبلاً چیزی آپلود شده باشد)
        public DocumentStatus? CurrentStatus { get; set; }
        public string? CurrentFilePath { get; set; }
        public string? CurrentValue { get; set; }
        public string? AdminNote { get; set; }
    }
}
