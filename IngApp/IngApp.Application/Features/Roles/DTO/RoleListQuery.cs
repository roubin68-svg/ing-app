// IngApp.Application/Features/Roles/DTO/RoleListQuery.cs
namespace IngApp.Application.Features.Roles.DTO
{
    /// <summary>
    /// Query استاندارد برای صفحه‌بندی، جستجو و مرتب‌سازی نقش‌ها
    /// </summary>
    public class RoleListQuery
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// جستجو روی Name و DisplayName
        /// </summary>
        public string? Search { get; set; }

        /// <summary>
        /// فیلتر بر اساس فعال/غیرفعال بودن نقش
        /// null = همه
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// نام فیلد مرتب‌سازی: "Name" | "DisplayName" | "IsActive"
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// آیا مرتب‌سازی نزولی باشد؟
        /// </summary>
        public bool SortDesc { get; set; } = false;
    }
}
