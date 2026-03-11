using InventoryApp.Application.DTOs.Inventory;

namespace InventoryApp.Application.DTOs.Admin;

public sealed record AdminStatsDto(
    int TotalUsers,
    int TotalInventories,
    int TotalItems,
    List<InventoryListItemDto> TotalInventoriesByItems,
    List<AdminUserDto> RecentUsers);
