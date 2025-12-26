using IngApp.Domain.Enums;

namespace IngApp.Application.Features.Suppliers.DTO
{
    public class SupplierListQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string? SortBy { get; set; }
        public bool SortDesc { get; set; }

        // فیلترها
        public string? BusinessName { get; set; }
        public string? userPhoneNumber { get; set; }
        public string? province { get; set; }
        public string? city { get; set; }

        public int? SupplierTypeId { get; set; }
        public VerificationStatus? VerificationStatus { get; set; }

    }
}
