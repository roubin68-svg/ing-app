using System;
using IngApp.Domain.Enums;

namespace IngApp.Domain.Entities.Suppliers
{
    public class SupplierVerificationHistory
    {
        public Guid Id { get; set; }

        public Guid SupplierProfileId { get; set; }
        public SupplierProfile SupplierProfile { get; set; } = null!;

        public VerificationStatus OldStatus { get; set; }
        public VerificationStatus NewStatus { get; set; }

        public string? AdminUserId { get; set; } // از Claim خوانده می‌شود
        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
