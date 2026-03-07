using InventoryApp.Domain.Enums;

namespace InventoryApp.Application.DTOs.Item;

public sealed record ItemFieldValueDto(
    int FieldId,
    string FieldTitle,
    FieldType FieldType,
    string? TextValue,
    decimal? NumericValue,
    bool? BooleanValue);
