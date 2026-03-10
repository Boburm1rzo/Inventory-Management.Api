using InventoryApp.Application.DTOs.Inventory;

namespace InventoryApp.Application.DTOs.Search;

public sealed record SearchResultDto(
    List<InventoryListItemDto> Inventories,
    List<ItemSearchResultDto> Items);