namespace InventoryApp.Application.DTOs;

public sealed record UserResponseDto(
    int Id,
    string Email,
    string DisplayName,
    string AvatarUrl);
