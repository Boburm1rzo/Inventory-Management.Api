using InventoryApp.Application.Common;
using InventoryApp.Domain.Entities;

namespace InventoryApp.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);
    Task<List<User>> SearchByNameOrEmailAsync(string query);
    Task<PagedResult<User>> GetPagedAsync(int page, int size);
    Task UpdateAstnc(User user);
}
