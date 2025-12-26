using IngApp.Application.Common.Interfaces.Products;
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Products.Attributes;
using IngApp.Application.Features.Products.DTO;
using Microsoft.AspNetCore.Mvc;

namespace IngApp.Api.Controllers.v1;

[ApiController]
[Route("api/v1/product-attribute-definitions")]
public class ProductAttributeDefinitionsController : ControllerBase
{
    private readonly IProductAttributeDefinitionService _service;

    public ProductAttributeDefinitionsController(IProductAttributeDefinitionService service)
    {
        _service = service;
    }

    // ------------------------------------------------------------
    // GET: Paging
    // ------------------------------------------------------------
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged(
        [FromQuery] ProductAttributeDefinitionListQuery filter)
    {
        var data = await _service.GetPagedAsync(filter);
        return Ok(ApiResult.Ok(data));
    }

    // ------------------------------------------------------------
    // GET: تمام Attribute Definition ها
    // ------------------------------------------------------------
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var data = await _service.GetAllAsync();
        return Ok(ApiResult.Ok(data));
    }

    // ------------------------------------------------------------
    // GET: فقط Active ها
    // ------------------------------------------------------------
    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var data = await _service.GetActiveAsync();
        return Ok(ApiResult.Ok(data));
    }

    // ------------------------------------------------------------
    // GET: دریافت با Id
    // ------------------------------------------------------------
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return Ok(ApiResult.Ok(item));
    }

    // ------------------------------------------------------------
    // POST: ایجاد
    // ------------------------------------------------------------
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductAttributeDefinitionRequest request)
    {
        var created = await _service.CreateAsync(request);
        return Ok(ApiResult.Ok(created));
    }

    // ------------------------------------------------------------
    // PUT: ویرایش
    // ------------------------------------------------------------
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateProductAttributeDefinitionRequest request)
    {
        var updated = await _service.UpdateAsync(id, request);
        return Ok(ApiResult.Ok(updated));
    }

    // ------------------------------------------------------------
    // PUT: فعال‌سازی
    // ------------------------------------------------------------
    [HttpPut("{id}/activate")]
    public async Task<IActionResult> Activate(int id)
    {
        await _service.ActivateAsync(id);
        return Ok(ApiResult.Ok());
    }

    // ------------------------------------------------------------
    // PUT: غیرفعال‌سازی
    // ------------------------------------------------------------
    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _service.DeactivateAsync(id);
        return Ok(ApiResult.Ok());
    }
}
