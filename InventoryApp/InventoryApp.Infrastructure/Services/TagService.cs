using InventoryApp.Application.DTOs.Tag;
using InventoryApp.Application.Interfaces;
using InventoryApp.Application.Mappers;
using InventoryApp.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Infrastructure.Services;

internal sealed class TagService(AppDbContext context) : ITagService
{
    public async Task<List<TagDto>> GetAsync(string? prefix = null)
    => await context.Tags
        .Where(x => string.IsNullOrEmpty(prefix) || x.Name.StartsWith(prefix))
        .OrderBy(x => x.Name)
        .Take(20)
        .Select(x => x.MapToDto())
        .ToListAsync();
}
