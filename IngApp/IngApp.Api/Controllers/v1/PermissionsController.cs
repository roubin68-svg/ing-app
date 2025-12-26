using IngApp.Application.Common.Models;
using IngApp.Application.Common.Interfaces.Permissions;
using IngApp.Application.Common.Security;
using IngApp.Application.Features.Permissions.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IngApp.Api.Controllers.v1
{
    [ApiController]
    [Route("api/v1/permissions")]
    [Authorize(Policy = Permissions.PermissionsModule.Manage)]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _service;

        public PermissionsController(IPermissionService service)
        {
            _service = service;
        }

        // ------------------------------------------
        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] PermissionListQuery request)
        {
            var result = await _service.GetPagedPermissionsAsync(request);
            return Ok(ApiResult.Ok(result));
        }

        // ------------------------------------------
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var items = await _service.GetAllPermissionsAsync();
            return Ok(ApiResult.Ok(items));
        }

        // ------------------------------------------
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _service.GetPermissionByIdAsync(id);
            return Ok(ApiResult.Ok(item));
        }

        // ------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Create(CreatePermissionRequest request)
        {
            var id = await _service.CreatePermissionAsync(request);
            return Ok(ApiResult.Ok(id));
        }

        // ------------------------------------------
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, UpdatePermissionRequest request)
        {
            var updated = await _service.UpdatePermissionAsync(id, request);
            return Ok(ApiResult.Ok(updated));
        }

        // ------------------------------------------
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeletePermissionAsync(id);
            return Ok(ApiResult.Ok());
        }

        // ------------------------------------------
        [HttpGet("{id:guid}/roles")]
        public async Task<IActionResult> GetRolesByPermission(Guid id)
        {
            var roles = await _service.GetRolesByPermissionIdAsync(id);
            return Ok(ApiResult.Ok(roles));
        }
    }
}
