using IngApp.Application.Common.Interfaces.Users;
using IngApp.Application.Common.Models;
using IngApp.Application.Common.Security;
using IngApp.Application.Features.Users.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IngApp.Api.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // -------------------- GET Paged List --------------------
        [HttpGet]
        [Authorize(Policy = Permissions.Users.View)]
        public async Task<IActionResult> GetPaged([FromQuery] UserListQueryDto filter)
        {
            var result = await _userService.GetPagedAsync(filter);
            return Ok(ApiResult.Ok(result));
        }

        // -------------------- GET By Id --------------------
        [HttpGet("{id:guid}")]
        [Authorize(Policy = Permissions.Users.View)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _userService.GetByIdAsync(id);

            if (user == null)
                return NotFound(ApiResult.Fail("کاربر مورد نظر یافت نشد."));

            return Ok(ApiResult.Ok(user));
        }

        // -------------------- CREATE User --------------------
        [HttpPost]
        [Authorize(Policy = Permissions.Users.Manage)]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            var created = await _userService.CreateAsync(dto);
            return Ok(ApiResult.Ok(created));
        }

        // -------------------- UPDATE User --------------------
        [HttpPut("{id:guid}")]
        [Authorize(Policy = Permissions.Users.Manage)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
        {
            await _userService.UpdateUserAsync(id, dto);
            return Ok(ApiResult.Ok());
        }

        // -------------------- CHANGE STATUS --------------------
        [HttpPut("{id:guid}/status")]
        [Authorize(Policy = Permissions.Users.Manage)]
        public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeUserStatusDto dto)
        {
            await _userService.ChangeStatusAsync(id, dto);
            return Ok(ApiResult.Ok());
        }

        // -------------------- ASSIGN ROLE --------------------
        [HttpPost("{id:guid}/roles")]
        [Authorize(Policy = Permissions.Users.Manage)]
        public async Task<IActionResult> AssignRole(Guid id, [FromBody] AssignRoleToUserDto dto)
        {
            await _userService.AssignRoleAsync(id, dto.RoleId);
            return Ok(ApiResult.Ok());
        }

        // -------------------- REMOVE ROLE --------------------
        [HttpDelete("{id:guid}/roles/{roleId:guid}")]
        [Authorize(Policy = Permissions.Users.Manage)]
        public async Task<IActionResult> RemoveRole(Guid id, Guid roleId)
        {
            await _userService.RemoveRoleAsync(id, roleId);
            return Ok(ApiResult.Ok());
        }
    }
}
