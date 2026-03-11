namespace InventoryApp.Application.DTOs.Admin;

public sealed record AdminUserDto(
    string Id,
    string DisplayName,
    string Email,
    string? AvatarUrl,
    bool IsBlocked,
    DateTime CreatedAt,
    int InventoryCount,
    int ItemCount);
