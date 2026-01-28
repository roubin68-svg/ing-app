namespace IngApp.Domain.Entities.Users;

using IngApp.Domain.Entities.Roles;

public class UserRole
{
    public Guid RoleId { get; set; }
    public Role? Role { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }
}
