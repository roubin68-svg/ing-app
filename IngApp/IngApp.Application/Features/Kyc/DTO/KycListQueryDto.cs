using IngApp.Domain.Enums;

namespace IngApp.Application.Features.Kyc.DTO
{
    public class KycListQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string? SortBy { get; set; }
        public bool SortDesc { get; set; }

        // فیلترها
        public Guid? UserId { get; set; }
        public int? AttributeDefinitionId { get; set; }
        public int? SupplierTypeId { get; set; }
        public DocumentStatus? Status { get; set; }

        public string? BusinessName { get; set; }
        public string? SupplierCode { get; set; }
    }
}
