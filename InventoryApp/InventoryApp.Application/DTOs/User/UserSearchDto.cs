namespace InventoryApp.Application.DTOs.User;

public sealed record UserSearchDto(
    string Id,
    string DisplayName,
    string Email,
    string? AvaterUrl);
