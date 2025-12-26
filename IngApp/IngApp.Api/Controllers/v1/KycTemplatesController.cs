using IngApp.Application.Common.Interfaces.Kyc;
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Kyc.DTO;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/kyc-templates")]
public class KycTemplatesController : ControllerBase
{
    private readonly IKycTemplateService _service;

    public KycTemplatesController(IKycTemplateService service)
    {
        _service = service;
    }

    [HttpGet("{supplierTypeId}")]
    public async Task<IActionResult> GetBySupplierType(int supplierTypeId)
    {
        var result = await _service.GetBySupplierTypeAsync(supplierTypeId);
        return Ok(ApiResult.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Upsert(CreateKycTemplateRequest request)
    {
        await _service.UpsertAsync(request);
        return Ok(ApiResult.Ok());
    }
}
