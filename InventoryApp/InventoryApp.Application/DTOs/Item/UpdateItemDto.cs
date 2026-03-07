namespace InventoryApp.Application.DTOs.Item;

public sealed record UpdateItemDto(
    byte[] RowVertion,
    List<CreateItemFieldValueDto> FieldValues);
