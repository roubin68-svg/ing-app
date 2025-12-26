using IngApp.Application.Common.Interfaces.Products;
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Products.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IngApp.Api.Controllers.v1;

[ApiController]
[Route("api/v1/product-categories")]
[Authorize]
public class ProductCategoriesController : ControllerBase
{
    private readonly IProductCategoryService _service;

    public ProductCategoriesController(IProductCategoryService service)
    {
        _service = service;
    }

    // -------------------------------------------------------
    // GET ALL PRODUCT CATEGORIES (FOR TREE VIEW)
    // -------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        var result = await _service.GetAllAsync();
        return Ok(ApiResult.Ok(result));
    }

    // -------------------------------------------------------
    // CREATE PRODUCT CATEGORY
    // -------------------------------------------------------
    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateProductCategoryRequest request)
    {
        var result = await _service.CreateAsync(request);
        return Ok(ApiResult.Ok(result));
    }

    // -------------------------------------------------------
    // UPDATE PRODUCT CATEGORY
    // -------------------------------------------------------
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync(
        int id,
        [FromBody] UpdateProductCategoryRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return Ok(ApiResult.Ok(result));
    }

    // -------------------------------------------------------
    // ACTIVATE PRODUCT CATEGORY
    // -------------------------------------------------------
    [HttpPut("{id:int}/activate")]
    public async Task<IActionResult> ActivateAsync(int id)
    {
        await _service.ActivateAsync(id);
        return Ok(ApiResult.Ok());
    }

    // -------------------------------------------------------
    // DEACTIVATE PRODUCT CATEGORY
    // -------------------------------------------------------
    [HttpPut("{id:int}/deactivate")]
    public async Task<IActionResult> DeactivateAsync(int id)
    {
        await _service.DeactivateAsync(id);
        return Ok(ApiResult.Ok());
    }
}
