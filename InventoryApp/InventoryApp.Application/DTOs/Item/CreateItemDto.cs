namespace InventoryApp.Application.DTOs.Item;

public sealed record CreateItemDto(
    List<CreateItemFieldValueDto> FieldValues);
