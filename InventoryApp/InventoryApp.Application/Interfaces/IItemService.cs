using InventoryApp.Application.DTOs.Item;
using InventoryApp.Domain.Common;

namespace InventoryApp.Application.Interfaces;

public interface IItemService
{
    Task<PagedResult<ItemListItemDto>> GetPagedAsync(int inventoryId, int page, int size);
    Task<ItemDto> GetByIdAsync(int inventoryId, int itemId);
    Task<ItemDto> CreateAsync(int inventoryId, CreateItemDto dto);
    Task<ItemDto> UpdateAsync(int inventoryId, int itemId, UpdateItemDto dto);
    Task DeleteAsync(int inventoryId, int itemId);
}
