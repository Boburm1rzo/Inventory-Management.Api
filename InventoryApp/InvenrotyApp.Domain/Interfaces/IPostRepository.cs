using InventoryApp.Domain.Entities;

namespace InventoryApp.Domain.Interfaces;

public interface IPostRepository
{
    Task<List<Post>> GetByInventoryAsync(int inventoryId);
    Task AddAsync(Post post);
}
