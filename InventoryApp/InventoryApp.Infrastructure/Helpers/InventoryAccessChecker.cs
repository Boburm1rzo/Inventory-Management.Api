using InventoryApp.Application.Interfaces;
using InventoryApp.Domain.Extentions;
using InventoryApp.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Infrastructure.Helpers;

internal sealed class InventoryAccessChecker(
    AppDbContext context,
    ICurrentUserService currentUserService)
{
    public async Task CheckAsync(int inventoryId, CancellationToken ct = default)
    {
        var inventory = await context.Inventories
            .FirstOrDefaultAsync(x => x.Id == inventoryId, ct)
            ?? throw new NotFoundException($"Inventory {inventoryId} not found.");

        if (inventory.OwnerId != currentUserService.UserId && !currentUserService.IsAdmin)
            throw new ForbiddenException("You don't have access.");
    }
}
