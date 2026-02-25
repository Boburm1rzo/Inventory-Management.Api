using InventoryApp.Application.Common;
using InventoryApp.Domain.Entities;

namespace InventoryApp.Domain.Interfaces;

public interface IInventoryRepository
{
    Task<Inventory?> GetByIdAsync(int id);
    Task<PagedResult<Inventory>> GetPagedAsync(int page, int size);
    Task<List<Inventory>> GetByTagAsync(string tagName);
    Task<List<Inventory>> GetLatestAsync(int count);
    Task<List<Inventory>> GetTopByItemCountAsync(int count);
    Task AddAsync(Inventory inventory);
    Task UpdateAsync(Inventory inventory);
    Task DeleteAsync(int id);
}
