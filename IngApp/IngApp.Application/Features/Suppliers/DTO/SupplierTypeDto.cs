namespace IngApp.Application.Features.Suppliers.DTO
{
    public class SupplierTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
