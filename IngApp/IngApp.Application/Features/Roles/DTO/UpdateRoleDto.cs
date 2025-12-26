namespace IngApp.Application.Features.Roles.DTO;

public class UpdateRoleDto
{
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
