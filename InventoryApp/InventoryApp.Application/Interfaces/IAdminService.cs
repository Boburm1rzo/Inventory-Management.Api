using InventoryApp.Application.DTOs.Admin;
using InventoryApp.Domain.Common;

namespace InventoryApp.Application.Interfaces;

public interface IAdminService
{
    Task<PagedResult<AdminUserDto>> GetUsersAsync(int page, int size, string? search);
    Task BlockUserAsync(string userId);
    Task UnblockUserAsync(string userId);
    Task DeleteUserAsync(string userId);
    Task<AdminStatsDto> GetStatsAsync();
}
