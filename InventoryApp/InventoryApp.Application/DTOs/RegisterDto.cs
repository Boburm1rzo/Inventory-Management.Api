namespace InventoryApp.Application.DTOs;

public sealed record RegisterDto(
    string DisplayName,
    string Email,
    string Password);
