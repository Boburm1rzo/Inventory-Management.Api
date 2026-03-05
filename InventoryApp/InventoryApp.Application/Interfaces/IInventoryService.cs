using InventoryApp.Application.DTOs.Inventory;
using InventoryApp.Domain.Common;

namespace InventoryApp.Application.Interfaces;

public interface IInventoryService
{
    Task<InventoryDto> GetByIdAsync(int id);
    Task<PagedResult<InventoryListItemDto>> GetPagedAsync(int page, int size);
    Task<InventoryDto> CreateAsync(CreateInventoryDto dto);
    Task<InventoryDto> UpdateAsync(int id, UpdateInventoryDto dto);
    Task DeleteAsync(int id);
}
