using InventoryApp.Application.DTOs.InventoryIdFormat;

namespace InventoryApp.Application.Interfaces;

public interface IInventoryIdFormatService
{
    Task<List<InventoryIdFormatPartDto>> GetPartsAsync(int inventoryId);
    Task<InventoryIdFormatPartDto> AddPartAsync(int inventoryId, CreateIdFormatPartDto dto);
    Task DeletePartAsync(int inventoryId, int partId);
    Task ReorderPartsAsync(int inventoryId, ReorderIdFormatPartsDto dto);
    Task<string> PreviewIdAsync(int inventoryId);
}
