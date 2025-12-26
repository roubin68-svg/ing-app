namespace IngApp.Application.Features.Permissions.DTO
{
    public class CreatePermissionRequest
    {
        public string Code { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
