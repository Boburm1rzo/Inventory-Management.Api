using InventoryApp.Domain.Common;
using InventoryApp.Domain.Entities;

namespace InventoryApp.Domain.Interfaces;

public interface IInventoryRepository
{
    Task<Inventory?> GetByIdAsync(int id);
    Task<PagedResult<Inventory>> GetPagedAsync(int page, int size, string? userId, bool isAdmin);
    Task AddAsync(Inventory inventory);
    Task UpdateAsync(Inventory inventory);
    Task DeleteAsync(int id);
}
