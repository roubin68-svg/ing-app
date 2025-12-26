using IngApp.Domain.Enums;

namespace IngApp.Application.Features.Kyc.DTO
{
    public class KycAttributeDefinitionListQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
         
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; }

        // فیلترها
        public string? DisplayName { get; set; }
        public KycDataType? DataType { get; set; }
        public bool? IsActive { get; set; }
    }
}
