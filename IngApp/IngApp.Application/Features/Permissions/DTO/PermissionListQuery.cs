// IngApp.Application/Features/Permissions/DTO/PermissionListQuery.cs
namespace IngApp.Application.Features.Permissions.DTO
{
    public class PermissionListQuery
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // جستجو روی Code و DisplayName
        public string? Search { get; set; }

        // فیلتر فعال/غیرفعال
        public bool? IsActive { get; set; }

        // "Code" | "DisplayName" | "IsActive"
        public string? SortBy { get; set; }

        public bool SortDesc { get; set; } = false;
    }
}
