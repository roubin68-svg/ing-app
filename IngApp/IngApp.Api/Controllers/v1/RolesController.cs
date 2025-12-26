using IngApp.Application.Common.Interfaces.Roles;
using IngApp.Application.Common.Models;
using IngApp.Application.Common.Security;
using IngApp.Application.Features.Roles.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IngApp.Api.Controllers.v1
{
    [ApiController]
    [Route("api/v1/roles")]
    [Authorize(Policy = Permissions.Roles.Manage)]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _service;

        public RolesController(IRoleService service)
        {
            _service = service;
        }

        // ----------------------------------------------------
        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] RoleListQuery query)
        {
            var result = await _service.GetPagedAsync(query);
            return Ok(ApiResult.Ok(result));
        }

        // ----------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _service.GetAllAsync();
            return Ok(ApiResult.Ok(roles));
        }

        // ----------------------------------------------------
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var dto = await _service.GetByIdAsync(id);
            return Ok(ApiResult.Ok(dto));
        }

        // ----------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Create(CreateRoleDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return Ok(ApiResult.Ok(result));
        }

        // ----------------------------------------------------
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, UpdateRoleDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            return Ok(ApiResult.Ok(updated));
        }

        // ----------------------------------------------------
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return Ok(ApiResult.Ok());
        }

        // ----------------------------------------------------
        [HttpPost("{id:guid}/permissions")]
        public async Task<IActionResult> AssignPermissions(Guid id, AssignPermissionsToRoleDto dto)
        {
            await _service.AssignPermissionsAsync(id, dto);
            return Ok(ApiResult.Ok());
        }
    }
}
