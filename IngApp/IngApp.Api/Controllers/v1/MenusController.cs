using IngApp.Application.Common.Interfaces.Menus;
using IngApp.Application.Common.Models;
using IngApp.Application.Common.Security;
using IngApp.Application.Features.Menus.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IngApp.Api.Controllers.v1
{
    [ApiController]
    [Route("api/v1/menus")]
    public class MenusController : ControllerBase
    {
        private readonly IMenuService _menuService;

        public MenusController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        // -------------------------------------------------------
        // 1) GET FULL MENU FOR ADMIN
        // -------------------------------------------------------
        [Authorize(Policy = Permissions.Menus.Manage)]
        [HttpGet("admin")]
        public async Task<IActionResult> GetAllForAdminAsync()
        {
            var result = await _menuService.GetAllForAdminAsync();
            return Ok(ApiResult.Ok(result));
        }

        // -------------------------------------------------------
        // 2) GET DYNAMIC MENU FOR CURRENT USER (ROLES + PERMS)
        // -------------------------------------------------------
        [Authorize]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyAsync()
        {
            var permissions = User.Claims
                .Where(c => c.Type == "permission")
                .Select(c => c.Value)
                .ToList();

            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            var result = await _menuService.GetMenuForUserAsync(permissions, roles);
            return Ok(ApiResult.Ok(result));
        }

        // -------------------------------------------------------
        // CREATE MENU ITEM
        // -------------------------------------------------------
        [Authorize(Policy = Permissions.Menus.Manage)]
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateMenuItemDto dto)
        {
            var result = await _menuService.CreateAsync(dto);
            return Ok(ApiResult.Ok(result));
        }

        // -------------------------------------------------------
        // UPDATE MENU ITEM
        // -------------------------------------------------------
        [Authorize(Policy = Permissions.Menus.Manage)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateMenuItemDto dto)
        {
            var result = await _menuService.UpdateAsync(id, dto);
            return Ok(ApiResult.Ok(result));
        }

        // -------------------------------------------------------
        // DELETE MENU ITEM
        // -------------------------------------------------------
        [Authorize(Policy = Permissions.Menus.Manage)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            await _menuService.DeleteAsync(id);
            return Ok(ApiResult.Ok());
        }

        // -------------------------------------------------------
        // CHANGE ORDER
        // -------------------------------------------------------
        [Authorize(Policy = Permissions.Menus.Manage)]
        [HttpPut("{id:int}/order")]
        public async Task<IActionResult> ChangeOrderAsync(int id, [FromBody] ChangeMenuOrderDto dto)
        {
            await _menuService.ChangeOrderAsync(id, dto.NewOrder);
            return Ok(ApiResult.Ok());
        }

        // -------------------------------------------------------
        // CHANGE PARENT
        // -------------------------------------------------------
        [Authorize(Policy = Permissions.Menus.Manage)]
        [HttpPut("{id:int}/parent")]
        public async Task<IActionResult> ChangeParentAsync(int id, [FromBody] ChangeMenuParentDto dto)
        {
            await _menuService.ChangeParentAsync(id, dto.ParentId);
            return Ok(ApiResult.Ok());
        }

        // -------------------------------------------------------
        // CHANGE PERMISSION
        // -------------------------------------------------------
        [Authorize(Policy = Permissions.Menus.Manage)]
        [HttpPut("{id:int}/permission")]
        public async Task<IActionResult> ChangePermissionAsync(int id, [FromBody] ChangeMenuPermissionDto dto)
        {
            await _menuService.ChangePermissionAsync(id, dto.PermissionCode);
            return Ok(ApiResult.Ok());
        }

        // -------------------------------------------------------
        // CHANGE STATUS
        // -------------------------------------------------------
        [Authorize(Policy = Permissions.Menus.Manage)]
        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> ChangeStatusAsync(int id, [FromBody] ChangeMenuStatusDto dto)
        {
            await _menuService.ChangeStatusAsync(id, dto.IsActive);
            return Ok(ApiResult.Ok());
        }
    }
}
