using InventoryApp.Domain.Entities;

namespace InventoryApp.Domain.Interfaces;

public interface ITagRepository
{
    Task<List<Tag>> GetByPrefixAsync(string prefix, int limit = 10);
    Task<Tag> GetOrCreateAsync(string name);
}
