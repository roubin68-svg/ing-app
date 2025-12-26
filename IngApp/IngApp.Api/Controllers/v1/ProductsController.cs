using IngApp.Application.Common.Interfaces.Products;
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Products.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IngApp.Api.Controllers.v1;

[ApiController]
[Route("api/v1/products")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    // ----------------------------------------------------
    // PAGED
    // ----------------------------------------------------
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged([FromQuery] ProductListQuery query)
    {
        var result = await _service.GetPagedAsync(query);
        return Ok(ApiResult.Ok(result));
    }

    // ----------------------------------------------------
    // GET BY ID
    // ----------------------------------------------------
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var dto = await _service.GetByIdAsync(id);
        return Ok(ApiResult.Ok(dto));
    }

    // ----------------------------------------------------
    // CREATE
    // ----------------------------------------------------
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var result = await _service.CreateAsync(request);
        return Ok(ApiResult.Ok(result));
    }

    // ----------------------------------------------------
    // UPDATE
    // ----------------------------------------------------
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest request)
    {
        var updated = await _service.UpdateAsync(id, request);
        return Ok(ApiResult.Ok(updated));
    }

    // ----------------------------------------------------
    // ACTIVATE
    // ----------------------------------------------------
    [HttpPut("{id:int}/activate")]
    public async Task<IActionResult> Activate(int id)
    {
        await _service.ActivateAsync(id);
        return Ok(ApiResult.Ok());
    }

    // ----------------------------------------------------
    // DEACTIVATE
    // ----------------------------------------------------
    [HttpPut("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _service.DeactivateAsync(id);
        return Ok(ApiResult.Ok());
    }
}
