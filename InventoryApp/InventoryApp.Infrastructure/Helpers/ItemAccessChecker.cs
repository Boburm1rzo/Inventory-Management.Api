using InventoryApp.Application.Interfaces;
using InventoryApp.Domain.Extentions;
using InventoryApp.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Infrastructure.Helpers;

internal sealed class ItemAccessChecker(
    AppDbContext context,
    ICurrentUserService currentUserService)
{
    public async Task CheckAsync(int inventoryId)
    {
        var inventory = await context.Inventories
            .Include(x => x.AccessList)
            .FirstOrDefaultAsync(x => x.Id == inventoryId)
            ?? throw new NotFoundException($"Inventory {inventoryId} not found.");

        var userId = currentUserService.UserId;
        var isAdmin = currentUserService.IsAdmin;

        var hasAccess = isAdmin
            || inventory.OwnerId == userId
            || inventory.AccessList.Any(a => a.UserId == userId)
            || (inventory.IsPublic && !string.IsNullOrEmpty(userId));

        if (!hasAccess)
            throw new ForbiddenException("You don't have access to add items.");
    }
}
