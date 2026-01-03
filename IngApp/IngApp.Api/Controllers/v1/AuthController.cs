using IngApp.Application.Common.Models;
using IngApp.Application.Common.Interfaces.Authentication;
using IngApp.Application.Features.Auth.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IngApp.Api.Controllers.v1;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    // ============================================
    // SEND OTP
    // ============================================
    [HttpPost("send-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
    {
        var result = await _auth.SendOtpAsync(request);
        return Ok(ApiResult.Ok(result, "کد ارسال شد"));
    }

    // ============================================
    // Verify OTP
    // ============================================
    [HttpPost("verify-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var result = await _auth.VerifyOtpAsync(request);
        return Ok(ApiResult.Ok(result, "ورود موفقیت‌آمیز بود"));
    }


    // ============================================
    // GET CURRENT USER INFO
    // ============================================
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var userId = Guid.Parse(User.Claims.First(c => c.Type == "uid").Value);

        var info = await _auth.GetUserInfoAsync(userId);
        return Ok(ApiResult.Ok(info));
    }

    // ============================================
    // UPDATE MY PROFILE
    // ============================================
    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateMyProfileRequest request)
    {
        var userId = Guid.Parse(User.Claims.First(c => c.Type == "uid").Value);

        await _auth.UpdateMyProfileAsync(userId, request);
        return Ok(ApiResult.Ok("پروفایل با موفقیت به‌روزرسانی شد."));
    }
}
