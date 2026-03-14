using InventoryApp.Application.DTOs.Inventory;
using InventoryApp.Domain.Entities;

namespace InventoryApp.Application.Mappers;

public static class InventoryMappings
{
    public static InventoryDto MapToDto(this Inventory inventory) => new(
        inventory.Id,
        inventory.Title,
        inventory.Description,
        inventory.ImageUrl,
        inventory.IsPublic,
        inventory.OwnerId,
        inventory.Owner?.DisplayName,
        inventory.CategoryId,
        inventory.Category?.Name,
        inventory.InventoryTags.Select(t => t.Tag.Name).ToList(),
        inventory.CreatedAt,
        inventory.RowVersion);

    public static InventoryListItemDto MapToListItemDto(this Inventory inventory) => new(
        inventory.Id,
        inventory.Title,
        inventory.Category?.Name,
        inventory.Owner.DisplayName,
        inventory.CreatedAt);

    public static Inventory MapToEntity(this CreateInventoryDto dto) => new()
    {
        Title = dto.Title,
        Description = dto.Description,
        ImageUrl = dto.ImageUrl,
        IsPublic = dto.IsPublic,
        CategoryId = dto.CategoryId,
        CreatedAt = DateTime.UtcNow
    };
}
