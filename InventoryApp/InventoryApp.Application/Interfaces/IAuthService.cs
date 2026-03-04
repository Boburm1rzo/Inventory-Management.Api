using InventoryApp.Application.DTOs;

namespace InventoryApp.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto request);
    Task<AuthResponseDto> LoginAsync(LoginDto request);
    Task<AuthResponseDto> ExternalLoginAsync();
    Task<UserResponseDto> GetCurrentUserAsync(string userId);
}
