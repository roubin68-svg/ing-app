namespace IngApp.Application.Features.Roles.DTO;

public class CreateRoleDto
{
    public string Name { get; set; } = string.Empty;        // انگلیسی
    public string DisplayName { get; set; } = string.Empty; // فارسی
    public string? Description { get; set; }
}
