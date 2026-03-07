using InventoryApp.Domain.Enums;

namespace InventoryApp.Application.DTOs.Field;

public sealed record InventoryFieldDto(
    int Id,
    string Title,
    string? Description,
    FieldType Type,
    bool DisplayInTable,
    int Order);
