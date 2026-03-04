namespace InventoryApp.Application.DTOs;

public record AuthResponseDto(
    string Token,
    string DisplayName,
    string Email);
