using InventoryApp.Application.DTOs.Tag;
using InventoryApp.Application.Interfaces;
using InventoryApp.Application.Mappers;
using InventoryApp.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryApp.Infrastructure.Services;

internal sealed class TagService(
    AppDbContext context,
    ILogger<TagService> logger) : ITagService
{
    public async Task<List<TagDto>> GetAsync(string? prefix = null)
    {
        if (!string.IsNullOrEmpty(prefix))
        {
            logger.LogInformation("Fetching tags with prefix: '{Prefix}'", prefix);
        }

        return await context.Tags
            .Where(x => string.IsNullOrEmpty(prefix) || x.Name.StartsWith(prefix))
            .OrderBy(x => x.Name)
            .Take(20)
            .Select(x => x.MapToDto())
            .ToListAsync();
    }
}