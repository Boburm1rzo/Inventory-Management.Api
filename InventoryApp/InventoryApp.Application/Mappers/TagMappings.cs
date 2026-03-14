using InventoryApp.Application.DTOs.Tag;
using InventoryApp.Domain.Entities;

namespace InventoryApp.Application.Mappers;

public static class TagMappings
{
    public static TagDto MapToDto(this Tag tag)
        => new(tag.Id, tag.Name);
}
