using IngApp.Domain.Enums;

namespace IngApp.Application.Features.Kyc.DTO
{
    public class CreateKycAttributeDefinitionRequest
    {
        public string DisplayName { get; set; } = null!;
        public string? Description { get; set; }

        public KycDataType DataType { get; set; } = KycDataType.File;
        public bool DefaultRequired { get; set; } = true;
        public bool IsActive { get; set; } = true;

    }
}
