namespace InventoryApp.Application.DTOs.Item;

public sealed record CreateItemFieldValueDto(
    int FieldId,
    string? TextValue,
    decimal? NumericValue,
    bool? BooleanValue);
