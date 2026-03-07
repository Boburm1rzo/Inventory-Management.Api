using InventoryApp.Application.DTOs.Item;
using InventoryApp.Application.Interfaces;
using InventoryApp.Domain.Common;

namespace InventoryApp.Infrastructure.Services;

internal sealed class ItemService : IItemService
{
    public Task<PagedResult<ItemListItemDto>> GetPagedAsync(int inventoryId, int page, int size)
    {
        throw new NotImplementedException();
    }

    public Task<ItemDto> GetByIdAsync(int inventoryId, int itemId)
    {
        throw new NotImplementedException();
    }

    public Task<ItemDto> CreateAsync(int inventoryId, CreateItemDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<ItemDto> UpdateAsync(int inventoryId, int itemId, UpdateItemDto dto)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(int inventoryId, int itemId)
    {
        throw new NotImplementedException();
    }
}
