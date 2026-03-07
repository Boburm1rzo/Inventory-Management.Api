using InventoryApp.Application.DTOs.InventoryIdFormat;
using InventoryApp.Domain.Entities;

namespace InventoryApp.Application.Mappers;

public static class InventoryIdFormatMappings
{
    public static InventoryIdFormatPartDto MapToDto(this InventoryIdFormatPart entity) => new(
        entity.Id,
        entity.Type,
        entity.Order,
        entity.Config);

    public static InventoryIdFormatPart MapToEntity(this CreateIdFormatPartDto dto) => new()
    {
        Type = dto.Type,
        Config = dto.Config
    };
}
