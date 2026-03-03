using InventoryApp.Application.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InventoryApp.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    [HttpPost("register")]
    public IActionResult Register(RegisterDto request)
    {
        return Ok();
    }

    [HttpPost("login")]
    public IActionResult Login(LoginDto request)
    {
        return Ok();
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserResponseDto>> Me()
    {
        var res = new UserResponseDto(1, "", "", "");
        return res;
    }

    [HttpGet("external-callback")]
    public async Task<IActionResult> ExternalCallback(string provider)
    {
        var ext = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);

        if (!ext.Succeeded || ext.Principal is null)
            return BadRequest($"{provider} auth failed.");

        var email = ext.Principal.FindFirstValue(ClaimTypes.Email);
        var name = ext.Principal.FindFirstValue(ClaimTypes.Name) ?? email ?? "User";

        if (string.IsNullOrWhiteSpace(email))
        {
            var pid = ext.Principal.FindFirstValue(ClaimTypes.Name) ?? Guid.NewGuid().ToString("N");
            email = $"{provider.ToLower()}_{pid}@no-email.local";
        }

        return Ok();
    }

    [HttpGet("login-facebook")]
    public IActionResult LoginFacebook()
        => Challenge(BuildProps("/api/auth/facebook-callback"), "Facebook");

    [HttpGet("login-google")]
    public IActionResult LoginGoogle()
       => Challenge(BuildProps("/api/auth/google-callback"), "Google");

    [HttpGet("google-callback")]
    public Task<IActionResult> GoogleCallback() => ExternalCallback("Google");

    [HttpGet("facebook-callback")]
    public Task<IActionResult> FacebookCallback() => ExternalCallback("Facebook");

    private AuthenticationProperties BuildProps(string redirectPath) =>
      new()
      {
          RedirectUri = redirectPath
      };
}
