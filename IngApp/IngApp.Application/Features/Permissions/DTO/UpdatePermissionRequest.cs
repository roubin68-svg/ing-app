namespace IngApp.Application.Features.Permissions.DTO
{
    public class UpdatePermissionRequest
    {
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
