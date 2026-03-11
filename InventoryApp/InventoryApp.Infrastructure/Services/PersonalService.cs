using InventoryApp.Application.DTOs.Personal;
using InventoryApp.Application.Interfaces;
using InventoryApp.Application.Mappers;
using InventoryApp.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Infrastructure.Services;

internal sealed class PersonalService(
    ICurrentUserService currentUserService,
    AppDbContext context) : IPersonalService
{
    public async Task<PersonalStatsDto> GetMyStatsAsync()
    {
        var userId = currentUserService.UserId;

        var inventories = await context.Inventories
            .Where(x => x.OwnerId == userId)
            .Include(x => x.Items)
            .ToListAsync();

        var totalInventories = inventories.Count;
        var totalItems = inventories.Sum(x => x.Items.Count);

        var totalLikes = await context.Likes
            .Where(x => x.Item.InventoryId == context.Inventories
                .Where(i => i.OwnerId == userId)
                .Select(i => i.Id)
                .FirstOrDefault())
            .CountAsync();

        var inventortDtos = inventories.Select(x => x.MapToListItemDto()).ToList();

        return new(totalInventories, totalItems, totalLikes, inventortDtos);
    }
}
