using System;
using IngApp.Domain.Enums;

namespace IngApp.Application.Features.Suppliers.DTO
{
    public class SupplierProfileDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public int SupplierTypeId { get; set; }
        public string SupplierTypeName { get; set; } = null!;

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

        public string VerificationStatus { get; set; } = null!;
        public string? RejectionReason { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UserPhoneNumber { get; set; }
    }
}
