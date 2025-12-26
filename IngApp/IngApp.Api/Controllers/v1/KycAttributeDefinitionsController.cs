using IngApp.Application.Common.Interfaces.Kyc;
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Kyc.DTO;
using Microsoft.AspNetCore.Mvc;

namespace IngApp.Api.Controllers.v1
{
    [ApiController]
    [Route("api/v1/kyc-attribute-definitions")]
    public class KycAttributeDefinitionsController : ControllerBase
    {
        private readonly IKycAttributeDefinitionService _service;

        public KycAttributeDefinitionsController(IKycAttributeDefinitionService service)
        {
            _service = service;
        }

        // ------------------------------------------------------------
        // GET: Paging
        // ------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] KycAttributeDefinitionListQueryDto filter)
        {
            var data = await _service.GetPagedAsync(filter);
            return Ok(ApiResult.Ok(data));
        }

        // ------------------------------------------------------------
        // GET: تمام Attribute Definition ها (بدون Paging)
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
        // GET: دریافت یک Item با ID
        // ------------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            return Ok(ApiResult.Ok(item));
        }

        // ------------------------------------------------------------
        // POST: ساخت Attribute Definition جدید
        // ------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateKycAttributeDefinitionRequest request)
        {
            var created = await _service.CreateAsync(request);
            return Ok(ApiResult.Ok(created));
        }

        // ------------------------------------------------------------
        // PUT: ویرایش Attribute Definition
        // ------------------------------------------------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateKycAttributeDefinitionRequest request)
        {
            var updated = await _service.UpdateAsync(id, request);
            return Ok(ApiResult.Ok(updated));
        }

        // ------------------------------------------------------------
        // PUT: فعال سازی
        // ------------------------------------------------------------
        [HttpPut("{id}/activate")]
        public async Task<IActionResult> Activate(int id)
        {
            await _service.ActivateAsync(id);
            return Ok(ApiResult.Ok());
        }

        // ------------------------------------------------------------
        // PUT: غیرفعال سازی
        // ------------------------------------------------------------
        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            await _service.DeactivateAsync(id);
            return Ok(ApiResult.Ok());
        }
    }
}
