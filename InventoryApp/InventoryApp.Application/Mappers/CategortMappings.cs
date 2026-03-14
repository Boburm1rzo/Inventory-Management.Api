using InventoryApp.Application.DTOs.Category;
using InventoryApp.Domain.Entities;

namespace InventoryApp.Application.Mappers;

public static class CategortMappings
{
    public static CategoryDto MapToDto(this Category category)
        => new(category.Id, category.Name);

    public static Category MapToEntity(this CreateCategoryDto dto)
        => new() { Name = dto.Name };
}
