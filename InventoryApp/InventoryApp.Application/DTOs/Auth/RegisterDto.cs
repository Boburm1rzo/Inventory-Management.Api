namespace InventoryApp.Application.DTOs.Auth;

public sealed record RegisterDto(
    string DisplayName,
    string Email,
    string Password);
