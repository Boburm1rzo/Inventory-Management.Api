namespace InventoryApp.Application.DTOs.Auth;

public record AuthResponseDto(
    string Token,
    string DisplayName,
    string Email);
