namespace InventoryApp.Application.DTOs.Auth;

public sealed record UserResponseDto(
    string Id,
    string Email,
    string DisplayName,
    string AvatarUrl,
    IList<string> Roles);
