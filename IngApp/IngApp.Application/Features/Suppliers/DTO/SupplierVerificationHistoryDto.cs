using System;
using IngApp.Domain.Enums;

namespace IngApp.Application.Features.Suppliers.DTO
{
    public class SupplierVerificationHistoryDto
    {
        public VerificationStatus OldStatus { get; set; }
        public VerificationStatus NewStatus { get; set; }

        public string? AdminUserId { get; set; }
        public string? AdminDisplayName { get; set; }
        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
