using IngApp.Application.Common.Interfaces.Suppliers;
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Suppliers.DTO;
using Microsoft.AspNetCore.Mvc;

namespace IngApp.Api.Controllers.v1;

[ApiController]
[Route("api/v1/suppliers/{userId:guid}/categories")]
public class SupplierCategoryAccessController : ControllerBase
{
    private readonly ISupplierCategoryAccessService _service;

    public SupplierCategoryAccessController(
        ISupplierCategoryAccessService service)
    {
        _service = service;
    }

    // --------------------------------------------------
    // GET: دریافت Categoryهای مجاز Supplier
    // --------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> Get(Guid userId)
    {
        var result = await _service.GetByUserIdAsync(userId);
        return Ok(ApiResult.Ok(result));
    }

    // --------------------------------------------------
    // POST: Sync کامل دسترسی Categoryها
    // --------------------------------------------------
    [HttpPost]
    public async Task<IActionResult> Sync(
        Guid userId,
        [FromBody] SyncSupplierCategoryAccessRequest request)
    {
        await _service.SyncAsync(userId, request.ProductCategoryIds);
        return Ok(ApiResult.Ok());
    }
}
