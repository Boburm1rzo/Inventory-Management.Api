using InventoryApp.Application.Common;
using InventoryApp.Domain.Entities;

namespace InventoryApp.Domain.Interfaces;

public interface IItemRepository
{
    Task<Item?> GetByIdAsync(int id);
    Task<PagedResult<Item>> GetByInventoryAsync(int inventoryId, int page, int size);
    Task<int> GetMaxSequenceAsync(int inventoryId);
    Task AddAsync(Item item);
    Task UpdateAsync(Item item);
    Task DeleteAsync(int id);
}
