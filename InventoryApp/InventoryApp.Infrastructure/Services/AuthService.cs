using InventoryApp.Application.DTOs;
using InventoryApp.Application.Interfaces;
using InventoryApp.Domain.Entities;
using InventoryApp.Domain.Extentions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace InventoryApp.Infrastructure.Services;

internal sealed class AuthService(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
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
        var info = await signInManager.GetExternalLoginInfoAsync()
            ?? throw new DomainException("External login info not found.");

        logger.LogInformation("External login callback. Provider={Provider}", info.LoginProvider);

        var user = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

        if (user is null)
        {
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            logger.LogInformation("External login user not linked yet. Provider={Provider} Email={Email}", info.LoginProvider, email);

            if (string.IsNullOrWhiteSpace(email))
                throw new DomainException("Email not found from external provider.");

            user = await userManager.FindByEmailAsync(email);

            if (user is null)
            {
                user = new User
                {
                    UserName = email,
                    Email = email,
                    DisplayName = info.Principal
                        .FindFirstValue(ClaimTypes.Name) ?? email,
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(user);
                logger.LogInformation("External user created. UserId={UserId} Email={Email}", user.Id, user.Email);
            }

            await userManager.AddLoginAsync(user, info);
            logger.LogInformation("External login linked. UserId={UserId} Provider={Provider}", user.Id, info.LoginProvider);
        }

        if (user.IsBlocked)
        {
            logger.LogWarning("External login blocked. UserId={UserId} Email={Email}", user.Id, user.Email);
            throw new DomainException("Your account has been blocked.");
        }

        var roles = await userManager.GetRolesAsync(user);
        var token = jwtService.GenerateToken(user, roles);

        logger.LogInformation("External login success. UserId={UserId} Provider={Provider}", user.Id, info.LoginProvider);

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
