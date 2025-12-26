namespace IngApp.Application.Features.Kyc.DTO
{
    public class CreateKycTemplateRequest
    {
        public int SupplierTypeId { get; set; }

        public List<KycTemplateRequirementRequest> Requirements { get; set; } = [];
    }

    public class KycTemplateRequirementRequest
    {
        public int AttributeDefinitionId { get; set; }
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
    }
}
