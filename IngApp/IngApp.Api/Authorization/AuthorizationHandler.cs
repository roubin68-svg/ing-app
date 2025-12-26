using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IngApp.Api.Authorization
{
    public class AuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            // اگر کاربر لاگین نیست
            if (context.User?.Identity == null || !context.User.Identity.IsAuthenticated)
                return Task.CompletedTask;

            // ================== 🔥 Bypass کامل برای Admin ==================

            // 1) روش استاندارد .NET
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // 2) هر Claimی که نقش Admin را نشان بدهد
            var isAdminByClaim = context.User.Claims.Any(c =>
                // انواع رایج claim برای نقش
                (c.Type == ClaimTypes.Role ||
                 c.Type == "role" ||
                 c.Type == "roles" ||
                 c.Type == "Role") &&
                string.Equals(c.Value, "Admin", System.StringComparison.OrdinalIgnoreCase));

            if (isAdminByClaim)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // ================== 🎫 چک کردن Permissionها برای سایر کاربران ==================
            var permissions = context.User.Claims
                .Where(c => c.Type == "permission")
                .Select(c => c.Value)
                .ToHashSet(System.StringComparer.OrdinalIgnoreCase);

            if (permissions.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
