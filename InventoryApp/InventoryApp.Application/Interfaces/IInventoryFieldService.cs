using InventoryApp.Application.DTOs.Field;

namespace InventoryApp.Application.Interfaces;

public interface IInventoryFieldService
{
    Task<List<InventoryFieldDto>> GetFieldsAsync(int inventoryId);
    Task<InventoryFieldDto> AddFieldAsync(int inventoryId, CreateInventoryFieldDto dto);
    Task<InventoryFieldDto> UpdateFieldAsync(int inventoryId, int fieldId, UpdateInventoryFieldDto dto);
    Task DeleteFieldAsync(int inventoryId, int fieldId);
    Task ReorderFieldsAsync(int inventoryId, ReorderFieldsDto dto);
}
