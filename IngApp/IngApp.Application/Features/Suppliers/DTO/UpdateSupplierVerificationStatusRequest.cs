using IngApp.Domain.Enums;

namespace IngApp.Application.Features.Suppliers.DTO
{
    public class UpdateSupplierVerificationStatusRequest
    {
        public VerificationStatus Status { get; set; }
        public string? Note { get; set; }
    }
}
