namespace InventoryApp.Application.DTOs.Field;

public sealed record UpdateInventoryFieldDto(
    int Id,
    string Title,
    string? Description,
    bool DisplatInTable);
