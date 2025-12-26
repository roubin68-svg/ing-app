using System;
using System.Collections.Generic;
using IngApp.Application.Features.Suppliers.DTO;
using IngApp.Domain.Enums;

namespace IngApp.Application.Features.Suppliers.DTO
{
    public class SupplierDetailDto
    {
        // ----------------------
        // Supplier Profile Info
        // ----------------------
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

        public string? ContactName { get; set; }
        public string? ContactPhone { get; set; }

        public VerificationStatus VerificationStatus { get; set; }
        public string? RejectionReason { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // ----------------------------------
        // KYC Summary (just counts)
        // ----------------------------------
        public int TotalDocuments { get; set; }
        public int PendingDocuments { get; set; }
        public int ApprovedDocuments { get; set; }
        public int RejectedDocuments { get; set; }

        public string UserPhoneNumber { get; set; }

        // ----------------------------------
        // History Lists
        // ----------------------------------
        public List<SupplierVerificationHistoryDto> VerificationHistory { get; set; } = new();
        public List<SupplierActivityLogDto> ActivityLogs { get; set; } = new();
    }
}
