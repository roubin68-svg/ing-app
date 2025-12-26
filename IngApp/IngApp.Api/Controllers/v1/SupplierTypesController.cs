using IngApp.Application.Common.Interfaces.Suppliers;
using IngApp.Application.Common.Models;
using IngApp.Application.Features.Suppliers.DTO;
using Microsoft.AspNetCore.Mvc;

namespace IngApp.Api.Controllers.v1
{
    [ApiController]
    [Route("api/v1/supplier-types")]
    public class SupplierTypesController : ControllerBase
    {
        private readonly ISupplierTypeService _service;

        public SupplierTypesController(ISupplierTypeService service)
        {
            _service = service;
        }

        // GET: Paging برای Admin
        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] SupplierTypeListQueryDto filter)
        {
            var data = await _service.GetPagedAsync(filter);
            return Ok(ApiResult.Ok(data));
        }

        // GET: همه نوع‌ها (برای DropDown و غیره)
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(ApiResult.Ok(data));
        }

        // GET: یک نوع خاص
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);

            if (item == null)
                return NotFound(ApiResult.Fail("نوع تأمین‌کننده یافت نشد."));

            return Ok(ApiResult.Ok(item));
        }

        // POST: ایجاد نوع جدید
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSupplierTypeRequest request)
        {
            var created = await _service.CreateAsync(request);
            return Ok(ApiResult.Ok(created));
        }

        // PUT: ویرایش
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSupplierTypeRequest request)
        {
            var updated = await _service.UpdateAsync(id, request);
            return Ok(ApiResult.Ok(updated));
        }

        // PUT: فعال‌سازی
        [HttpPut("{id:int}/activate")]
        public async Task<IActionResult> Activate(int id)
        {
            await _service.ActivateAsync(id);
            return Ok(ApiResult.Ok());
        }

        // PUT: غیرفعال‌سازی
        [HttpPut("{id:int}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            await _service.DeactivateAsync(id);
            return Ok(ApiResult.Ok());
        }
    }
}
