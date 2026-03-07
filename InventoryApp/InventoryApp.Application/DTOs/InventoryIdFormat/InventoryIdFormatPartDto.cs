using InventoryApp.Domain.Enums;

namespace InventoryApp.Application.DTOs.InventoryIdFormat;

public sealed record InventoryIdFormatPartDto(
    int Id,
    IdFormatPartType Type,
    int Order,
    string? Config);
