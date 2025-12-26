using System;

namespace IngApp.Application.Features.Suppliers.DTO
{
    public class SupplierActivityLogDto
    {
        public string ActionType { get; set; } = null!;
        public string? MetadataJson { get; set; }

        public string? UserId { get; set; }
        public string? AdminUserId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
