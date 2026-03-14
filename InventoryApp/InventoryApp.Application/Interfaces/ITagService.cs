using InventoryApp.Application.DTOs.Tag;

namespace InventoryApp.Application.Interfaces;

public interface ITagService
{
    Task<List<TagDto>> GetAsync(string? prefix = null);
}
