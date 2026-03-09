namespace InventoryApp.Application.DTOs.Item;

public sealed record UpdateItemDto(
    byte[] RowVersion,
    List<CreateItemFieldValueDto> FieldValues);
