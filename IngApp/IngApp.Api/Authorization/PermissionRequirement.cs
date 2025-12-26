using Microsoft.AspNetCore.Authorization;

namespace IngApp.Api.Authorization
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }

        // اگر نیاز به مقدار ثابت بود:
        // public static readonly PermissionRequirement Products_ViewOwn = 
        //      new PermissionRequirement(Permissions.Products.ViewOwn);
    }
}
