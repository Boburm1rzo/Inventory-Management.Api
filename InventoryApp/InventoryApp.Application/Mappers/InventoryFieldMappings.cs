using InventoryApp.Application.DTOs.Field;
using InventoryApp.Domain.Entities;

namespace InventoryApp.Application.Mappers;

public static class InventoryFieldMappings
{
    public static InventoryFieldDto MapToDto(this InventoryField inventoryField) => new(
        inventoryField.Id,
        inventoryField.Title,
        inventoryField.Description,
        inventoryField.Type,
        inventoryField.DisplayInTable,
        inventoryField.Order);

    public static InventoryField MapToEntity(this CreateInventoryFieldDto dto) => new()
    {
        Title = dto.Title,
        Description = dto.Description,
        Type = dto.Type,
        DisplayInTable = dto.DisplatInTable,
        Order = dto.Order
    };
}
