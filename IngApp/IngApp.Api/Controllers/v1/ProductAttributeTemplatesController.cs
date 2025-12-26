using IngApp.Application.Common.Interfaces.Products;
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Products.DTO;
using Microsoft.AspNetCore.Mvc;

namespace IngApp.Api.Controllers.v1;

[ApiController]
[Route("api/v1/product-attribute-templates")]
public class ProductAttributeTemplatesController : ControllerBase
{
    private readonly IProductAttributeTemplateService _service;

    public ProductAttributeTemplatesController(IProductAttributeTemplateService service)
    {
        _service = service;
    }

    [HttpGet("{productId:int}")]
    public async Task<IActionResult> GetByProduct(int productId)
    {
        var result = await _service.GetByProductAsync(productId);
        return Ok(ApiResult.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Upsert(CreateProductAttributeTemplateRequest request)
    {
        await _service.UpsertAsync(request);
        return Ok(ApiResult.Ok());
    }
}
