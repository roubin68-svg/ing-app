using IngApp.Domain.Enums;

namespace IngApp.Application.Features.Offers.DTO;

public class AdminOfferListItemDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;

    public int ProductCategoryId { get; set; }
    public string ProductCategoryName { get; set; } = null!;

    public decimal TotalPrice { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = null!;

    public OfferStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? RejectedReason { get; set; }

    // Supplier Info
    public Guid SupplierUserId { get; set; }
    public string SupplierBusinessName { get; set; } = null!;
    public string? SupplierPhoneNumber { get; set; }

    // آمار کلیک‌ها
    public int ViewCount { get; set; }
    public int ContactClickCount { get; set; }
}












