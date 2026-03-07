namespace InventoryApp.Application.DTOs.Item;

public sealed record ItemDto(
    int Id,
    string CustomId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string CreatedBy,
    List<ItemFieldValueDto> FieldValues);

