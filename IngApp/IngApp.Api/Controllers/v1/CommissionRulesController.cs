using IngApp.Application.Common.Interfaces.Financial;
using IngApp.Application.Common.Models;
using IngApp.Application.Common.Security;
using IngApp.Application.Features.Financial.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IngApp.Api.Controllers.v1;

[ApiController]
[Route("api/v1/commission-rules")]
[Authorize]
public class CommissionRulesController : ControllerBase
{
    private readonly ICommissionRuleService _service;

    public CommissionRulesController(ICommissionRuleService service)
    {
        _service = service;
    }

    // GET: دریافت لیست تمام قوانین پورسانت
    [HttpGet]
    [Authorize(Policy = Permissions.Financial.Manage)]
    public async Task<IActionResult> GetAll()
    {
        var rules = await _service.GetAllAsync();
        return Ok(ApiResult.Ok(rules));
    }

    // GET: دریافت یک قانون پورسانت بر اساس ID
    [HttpGet("{id:int}")]
    [Authorize(Policy = Permissions.Financial.Manage)]
    public async Task<IActionResult> GetById(int id)
    {
        var rule = await _service.GetByIdAsync(id);
        if (rule == null)
            return NotFound(ApiResult.Fail("قانون پورسانت یافت نشد."));
        
        return Ok(ApiResult.Ok(rule));
    }

    // POST: ایجاد قانون پورسانت جدید
    [HttpPost]
    [Authorize(Policy = Permissions.Financial.Manage)]
    public async Task<IActionResult> Create([FromBody] CreateCommissionRuleDto dto)
    {
        var rule = await _service.CreateAsync(dto);
        return Ok(ApiResult.Ok(rule));
    }

    // PUT: به‌روزرسانی قانون پورسانت
    [HttpPut("{id:int}")]
    [Authorize(Policy = Permissions.Financial.Manage)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCommissionRuleDto dto)
    {
        var rule = await _service.UpdateAsync(id, dto);
        return Ok(ApiResult.Ok(rule));
    }

    // DELETE: حذف قانون پورسانت
    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.Financial.Manage)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResult.Ok());
    }
}

