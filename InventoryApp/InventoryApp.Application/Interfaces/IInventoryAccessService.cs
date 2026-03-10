using InventoryApp.Application.DTOs.Access;
using InventoryApp.Application.DTOs.User;

namespace InventoryApp.Application.Interfaces;

public interface IInventoryAccessService
{
    Task<List<InventoryAccessDto>> GetAccessListAsync(int inventoryId);
    Task AddAccessAsync(int inventoryId, AddAccessDto dto);
    Task RemoveAccessAsync(int inventoryId, string userId);
    Task<List<UserSearchDto>> SearchUsersAsync(string query);
}
