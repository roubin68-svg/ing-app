using System;
using IngApp.Domain.Enums;

namespace IngApp.Application.Features.Kyc.DTO
{
    public class UserDocumentDto
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public int AttributeDefinitionId { get; set; }

        // اطلاعات فیلد (AttributeDefinition)
        public string AttributeDisplayName { get; set; } = null!;
        public KycDataType DataType { get; set; }

        // مقدار/فایل
        public string? Value { get; set; }
        public string? FilePath { get; set; }

        // وضعیت
        public DocumentStatus Status { get; set; }
        public string? AdminNote { get; set; }

        // تاریخ‌ها
        public DateTime UploadedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}
