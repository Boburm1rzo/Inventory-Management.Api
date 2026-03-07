using InventoryApp.Domain.Common;
using InventoryApp.Domain.Entities;

namespace InventoryApp.Domain.Interfaces;

public interface IItemRepository
{
    Task<PagedResult<Item>> GetPagedAsync(int inventoryId, int page, int size);
    Task<Item?> GetByIdAsync(int inventoryId, int itemId);
    Task AddAsync(Item item);
    Task UpdateAsync(Item item);
    Task DeleteAsync(int inventoryId, int itemId);
}
