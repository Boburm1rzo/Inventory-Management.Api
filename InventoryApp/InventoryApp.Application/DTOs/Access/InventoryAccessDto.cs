namespace InventoryApp.Application.DTOs.Access;

public sealed record InventoryAccessDto(
    string UserId,
    string DisplayName,
    string? AvatarUrl,
    DateTime GrantedAt);
