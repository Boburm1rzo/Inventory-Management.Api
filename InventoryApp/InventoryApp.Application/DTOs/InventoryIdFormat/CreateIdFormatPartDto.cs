using InventoryApp.Domain.Enums;

namespace InventoryApp.Application.DTOs.InventoryIdFormat;

public sealed record CreateIdFormatPartDto(
    IdFormatPartType Type,
    string? config);
