using IngApp.Domain.Entities.Permissions;
using System;

namespace IngApp.Domain.Entities.Roles;

public class RolePermission
{
    public Guid RoleId { get; set; }
    public Role? Role { get; set; }

    public Guid PermissionId { get; set; }

    public Permission? Permission { get; set; }
}
