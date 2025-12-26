using System;

namespace IngApp.Domain.Entities.Suppliers
{
    public class SupplierActivityLog
    {
        public Guid Id { get; set; }

        public Guid SupplierProfileId { get; set; }
        public SupplierProfile SupplierProfile { get; set; } = null!;

        public string ActionType { get; set; } = null!; // e.g. "PROFILE_UPDATED", "STATUS_CHANGED"
        public string? MetadataJson { get; set; }        // optional details

        public string? UserId { get; set; }              // who triggered (Supplier)
        public string? AdminUserId { get; set; }         // OR Admin

        public DateTime CreatedAt { get; set; }
    }
}
