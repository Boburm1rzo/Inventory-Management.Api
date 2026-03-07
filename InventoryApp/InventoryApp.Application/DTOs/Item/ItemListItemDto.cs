namespace InventoryApp.Application.DTOs.Item;

public sealed record ItemListItemDto(
    int Id,
    string CustomId,
    DateTime CreatedAt,
    string CreatedBy,
    List<ItemFieldValueDto> FieldValues);
