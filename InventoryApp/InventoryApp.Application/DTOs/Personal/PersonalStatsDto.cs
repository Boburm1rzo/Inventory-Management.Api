using InventoryApp.Application.DTOs.Inventory;

namespace InventoryApp.Application.DTOs.Personal;

public sealed record PersonalStatsDto(
    int TotalInventories,
    int TotalItems,
    int TotalLikes,
    List<InventoryListItemDto> Inventories);
