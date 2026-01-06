using IngApp.Domain.Entities.Users; // مسیر دقیق User در پروژه
using IngApp.Domain.Enums;
using System;

namespace IngApp.Domain.Entities.Suppliers
{
    public class SupplierProfile
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public User User { get; set; } = null!;

        public int SupplierTypeId { get; set; }
        public SupplierType SupplierType { get; set; } = null!;

        public string BusinessName { get; set; } = null!;
        public string? NationalId { get; set; }
        public string? LicenseNumber { get; set; }

        public string? Province { get; set; }
        public string? City { get; set; }
        public string? Address { get; set; }

        public BusinessType? BusinessType { get; set; }
        public string? ContactName { get; set; }
        public ContactPosition? ContactPosition { get; set; }
        public string? ContactMobile { get; set; }
        public string? ContactPhone { get; set; }

        public VerificationStatus VerificationStatus { get; set; }
        public string? RejectionReason { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
