using IngApp.Domain.Enums;

namespace IngApp.Application.Features.Kyc.DTO
{
    public class KycTemplateItemDto
    {
        public int AttributeDefinitionId { get; set; }

        public string DisplayName { get; set; }

        public int DataType { get; set; }

        public bool IsRequired { get; set; }

        public int SortOrder { get; set; }
    }


}
