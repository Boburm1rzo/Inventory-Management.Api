using InventoryApp.Application.DTOs.Auth;

namespace InventoryApp.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto request);
    Task<AuthResponseDto> LoginAsync(LoginDto request);
    Task<AuthResponseDto> ExternalLoginAsync();
    Task<UserResponseDto> GetCurrentUserAsync(string userId);
}
