using IngApp.Domain.Enums;

namespace IngApp.Application.Features.Kyc.DTO
{
    public class ReviewKycDocumentRequest
    {
        public DocumentStatus Status { get; set; }
        public string? AdminNote { get; set; }
    }
}
