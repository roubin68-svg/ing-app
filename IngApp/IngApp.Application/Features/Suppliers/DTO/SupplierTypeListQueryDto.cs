namespace IngApp.Application.Features.Suppliers.DTO
{
    public class SupplierTypeListQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string? Name { get; set; }
        public bool? IsActive { get; set; }

        public string? SortBy { get; set; }
        public bool SortDesc { get; set; }
    }
}
