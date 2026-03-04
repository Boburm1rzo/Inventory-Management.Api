using InventoryApp.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace InventoryApp.Infrastructure.Services;

internal sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string UserId =>
        httpContextAccessor.HttpContext?.User
        .FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    public string Email =>
        httpContextAccessor.HttpContext.User
        .FindFirstValue(ClaimTypes.Email) ?? string.Empty;

    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext.User.Identity?.IsAuthenticated ?? false;
}
