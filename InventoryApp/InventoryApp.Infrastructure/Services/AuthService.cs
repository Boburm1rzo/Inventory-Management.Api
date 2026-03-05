using InventoryApp.Application.DTOs.Auth;
using InventoryApp.Application.Interfaces;
using InventoryApp.Domain.Entities;
using InventoryApp.Domain.Extentions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace InventoryApp.Infrastructure.Services;

internal sealed class AuthService(
    UserManager<User> userManager,
    IHttpContextAccessor httpContextAccessor,
    IJwtTokenService jwtService,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<AuthResponseDto> RegisterAsync(RegisterDto request)
    {
        logger.LogInformation("Register attempt. Email={Email} DisplayName={DisplayName}", request.Email, request.DisplayName);

        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(x => x.Description));

            logger.LogWarning("Register failed. Email={Email} Errors={Errors}", request.Email, errors);

            throw new DomainException(errors);
        }

        logger.LogInformation("User created successfully. UserId={UserId} Email={Email}", user.Id, user.Email);

        var roles = await userManager.GetRolesAsync(user);
        var token = jwtService.GenerateToken(user, roles);

        return new AuthResponseDto(token, user.DisplayName, user.Email);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto request)
    {
        logger.LogInformation("Login attempt. Email={Email}", request.Email);

        var user = await userManager.FindByEmailAsync(request.Email)
            ?? throw new DomainException("Invalid email or password.");

        if (user.IsBlocked)
        {
            logger.LogWarning("Login blocked. UserId={UserId} Email={Email}", user.Id, user.Email);
            throw new DomainException("Your account has been blocked.");
        }

        var isValid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!isValid)
        {
            logger.LogWarning("Login failed (invalid password). UserId={UserId} Email={Email}", user.Id, user.Email);
            throw new DomainException("Invalid email or password.");
        }

        var roles = await userManager.GetRolesAsync(user);
        var token = jwtService.GenerateToken(user, roles);

        logger.LogInformation("Login success. UserId={UserId} Email={Email}", user.Id, user.Email);

        return new AuthResponseDto(token, user.DisplayName, user.Email!);
    }

    public async Task<AuthResponseDto> ExternalLoginAsync()
    {
        var httpContext = httpContextAccessor.HttpContext
             ?? throw new DomainException("Context not found.");

        var authResult = await httpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);

        if (!authResult.Succeeded || authResult.Principal == null)
        {
            logger.LogWarning("External authentication failed: {Reason}", authResult.Failure?.Message);
            throw new DomainException("External login info not found.");
        }

        var principal = authResult.Principal;
        var loginProvider = authResult.Properties?.Items[".AuthScheme"] ?? "External";
        var providerKey = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new DomainException("Provider key not found.");

        logger.LogInformation("External login callback. Provider={Provider}", loginProvider);

        var user = await userManager.FindByLoginAsync(loginProvider, providerKey);

        if (user is null)
        {
            var email = principal.FindFirstValue(ClaimTypes.Email);
            logger.LogInformation("External login user not linked. Provider={Provider} Email={Email}", loginProvider, email);

            if (string.IsNullOrWhiteSpace(email))
                throw new DomainException("Email not provided by external service.");

            user = await userManager.FindByEmailAsync(email);

            if (user is null)
            {
                user = new User
                {
                    UserName = email,
                    Email = email,
                    DisplayName = principal.FindFirstValue(ClaimTypes.Name) ?? email,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    throw new DomainException(string.Join(", ", createResult.Errors.Select(e => e.Description)));
                }

                logger.LogInformation("New user created via {Provider}. UserId={UserId}", loginProvider, user.Id);
            }

            var info = new UserLoginInfo(loginProvider, providerKey, loginProvider);
            await userManager.AddLoginAsync(user, info);
        }

        if (user.IsBlocked)
        {
            throw new DomainException("Your account has been blocked.");
        }

        await httpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        var roles = await userManager.GetRolesAsync(user);
        var token = jwtService.GenerateToken(user, roles);

        return new AuthResponseDto(token, user.DisplayName, user.Email!);
    }

    public async Task<UserResponseDto> GetCurrentUserAsync(string userId)
    {
        logger.LogInformation("GetCurrentUser. UserId={UserId}", userId);

        var user = await userManager.FindByIdAsync(userId)
            ?? throw new DomainException("User not found.");

        var roles = await userManager.GetRolesAsync(user);

        return new UserResponseDto(
            user.Id,
            user.Email!,
            user.DisplayName,
            user.AvatarUrl!,
            roles);
    }
}
