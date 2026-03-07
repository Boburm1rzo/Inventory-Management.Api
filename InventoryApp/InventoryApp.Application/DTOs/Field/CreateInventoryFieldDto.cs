using InventoryApp.Domain.Enums;

namespace InventoryApp.Application.DTOs.Field;

public sealed record CreateInventoryFieldDto(
    string Title,
    string? Description,
    FieldType Type,
    bool DisplatInTable,
    int Order);
