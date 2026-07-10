using HRMS.API.Services;
using HRMS.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HRMS.API.Models;

namespace HRMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // [HttpPost("register")]
        // public async Task<ActionResult<AuthTokenResponseDto>> Register([FromBody] RegisterModel model)
        // {
        //     if (!ModelState.IsValid)
        //         return BadRequest(ModelState);

        //     var result = await _authService.RegisterAsync(model);
        //     if (!result.Success)
        //     {
        //         return BadRequest((result.ErrorMessage ?? "Registration failed.").ToMessageDto());
        //     }

        //     return Ok(new AuthTokenResponseDto { Token = result.Token, Message = "Registration successful" });
        // }

        [HttpPost("login")]
        public async Task<ActionResult<AuthSessionDto>> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(model);
            if (!result.Success)
            {
                return Unauthorized((result.ErrorMessage ?? "Invalid credentials").ToMessageDto());
            }

            return Ok(new AuthSessionDto
            {
                Token = result.Token,
                User = result.User!.ToDto(),
                Employee = result.Employee.ToAuthEmployeeDto(),
            });
        }

        [AllowAnonymous]
        [HttpPost("request-password-reset-otp")]
        public async Task<ActionResult<MessageDto>> RequestPasswordResetOtp([FromBody] RequestPasswordResetOtpModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _authService.RequestPasswordResetOtpAsync(model);

            // Keep response generic to avoid exposing which emails exist.
            return Ok("If an account exists for this email, a reset code has been sent.".ToMessageDto());
        }

        [AllowAnonymous]
        [HttpPost("reset-password-with-otp")]
        public async Task<ActionResult<MessageDto>> ResetPasswordWithOtp([FromBody] ResetPasswordWithOtpModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ResetPasswordWithOtpAsync(model);
            if (!result.Success)
            {
                return BadRequest((result.ErrorMessage ?? "Unable to reset password.").ToMessageDto());
            }

            return Ok("Password reset successful. You can now sign in with your new password.".ToMessageDto());
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<AuthSessionDto>> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _authService.GetCurrentUserAsync(userId);
            if (result.User == null)
            {
                return NotFound();
            }

            return Ok(new AuthSessionDto
            {
                User = result.User.ToDto(),
                Employee = result.Employee.ToAuthEmployeeDto(),
            });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<ActionResult<MessageDto>> ChangePassword([FromBody] ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _authService.ChangePasswordAsync(userId, model);
            if (!result.Success)
            {
                return BadRequest((result.ErrorMessage ?? "Unable to change password.").ToMessageDto());
            }

            return Ok("Password changed successfully.".ToMessageDto());
        }
    }
}
