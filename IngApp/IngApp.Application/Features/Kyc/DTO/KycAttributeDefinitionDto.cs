using IngApp.Domain.Enums;

namespace IngApp.Application.Features.Kyc.DTO
{
    public class KycAttributeDefinitionDto
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = null!;
        public string? Description { get; set; }

        public KycDataType DataType { get; set; }
        public bool DefaultRequired { get; set; }

        public bool IsActive { get; set; }
    }
}
