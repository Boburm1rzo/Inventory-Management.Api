using InventoryApp.Application.DTOs;
using InventoryApp.Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryApp.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    IAuthService authService,
    ICurrentUserService currentUserService,
    IConfiguration configuration,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto request)
    {
        logger.LogInformation("HTTP Register request. Email={Email}", request.Email);
        var result = await authService.RegisterAsync(request);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto request)
    {
        logger.LogInformation("HTTP Login request. Email={Email}", request.Email);
        var result = await authService.LoginAsync(request);
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserResponseDto>> Me()
    {
        var userId = currentUserService.UserId;
        logger.LogInformation("HTTP Me request. UserId={UserId}", userId);

        var result = await authService.GetCurrentUserAsync(userId);
        return Ok(result);
    }

    [HttpGet("login-google")]
    public IActionResult LoginGoogle()
    {
        var redirectUri = $"{Request.Scheme}://{Request.Host}/api/auth/google-callback";
        logger.LogInformation("Start Google login. RedirectUri={RedirectUri}", redirectUri);

        var props = new AuthenticationProperties { RedirectUri = redirectUri };
        return Challenge(props, "Google");
    }

    [HttpGet("login-facebook")]
    public IActionResult LoginFacebook()
    {
        var redirectUri = $"{Request.Scheme}://{Request.Host}/api/auth/facebook-callback";
        logger.LogInformation("Start Facebook login. RedirectUri={RedirectUri}", redirectUri);

        var props = new AuthenticationProperties { RedirectUri = redirectUri };
        return Challenge(props, "Facebook");
    }

    [HttpGet("google-callback")]
    public async Task<IActionResult> GoogleCallback()
    {
        logger.LogInformation("Google callback hit.");

        var result = await authService.ExternalLoginAsync();
        var frontendUrl = configuration["Frontend:Url"];

        logger.LogInformation("Google callback success. Redirecting to frontend.");
        return Redirect($"{frontendUrl}/auth/callback?token={result.Token}");
    }

    [HttpGet("facebook-callback")]
    public async Task<IActionResult> FacebookCallback()
    {
        logger.LogInformation("Facebook callback hit.");

        var result = await authService.ExternalLoginAsync();
        var frontendUrl = configuration["Frontend:Url"];

        logger.LogInformation("Facebook callback success. Redirecting to frontend.");
        return Redirect($"{frontendUrl}/auth/callback?token={result.Token}");
    }
}
