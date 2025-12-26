namespace IngApp.Application.Features.Permissions.DTO
{
    public class PermissionRoleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;        // Role System Name
        public string DisplayName { get; set; } = string.Empty; // Role UI Name
    }
}
