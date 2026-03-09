using InventoryApp.Application.DTOs.Item;
using InventoryApp.Domain.Entities;

namespace InventoryApp.Application.Mappers;

public static class ItemMappings
{
    public static ItemDto MapToDto(this Item item) => new(
        item.Id,
        item.CustomId,
        item.CreatedAt,
        item.UpdatedAt,
        item.CreatedBy.DisplayName,
        [.. item.FieldValues.Select(x => x.MapToDto())]);

    public static ItemFieldValueDto MapToDto(this ItemFieldValue fieldValue) => new(
        fieldValue.Id,
        fieldValue.Field.Title,
        fieldValue.Field.Type,
        fieldValue.TextValue,
        fieldValue.NumericValue,
        fieldValue.BooleanValue);

    public static ItemListItemDto MapToListDto(this Item item) => new(
        item.Id,
        item.CustomId,
        item.CreatedAt,
        item.CreatedBy.DisplayName,
        [.. item.FieldValues.Select(x => x.MapToDto())]);

    public static Item MapToEntity(this CreateItemDto dto) => new()
    {
        FieldValues = [.. dto.FieldValues.Select(x => x.MapToEntity())]
    };

    public static ItemFieldValue MapToEntity(this CreateItemFieldValueDto dto) => new()
    {
        FieldId = dto.FieldId,
        TextValue = dto.TextValue,
        NumericValue = dto.NumericValue,
        BooleanValue = dto.BooleanValue
    };
}
