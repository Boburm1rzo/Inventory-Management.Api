using InventoryApp.Application.Interfaces;
using InventoryApp.Domain.Extentions;
using InventoryApp.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Infrastructure.Helpers;

public sealed class AccessChecker(
    ICurrentUserService currentUserService,
    AppDbContext context)
{
    public async Task CheckInventoryAsync(int inventoryId)
    {
        var inventory = await context.Inventories
            .FirstOrDefaultAsync(x => x.Id == inventoryId)
            ?? throw new NotFoundException($"Inventory {inventoryId} not found.");

        if (inventory.OwnerId != currentUserService.UserId && !currentUserService.IsAdmin)
            throw new ForbiddenException("You don't have access.");
    }

    public async Task CheckItemAsync(int inventoryId)
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

    public async Task CheckOwnerAsync(int inventoryId)
    {
        var inventory = await context.Inventories
            .FirstOrDefaultAsync(x => x.Id == inventoryId)
            ?? throw new NotFoundException($"Inventory {inventoryId} not found.");

        if (inventory.OwnerId != currentUserService.UserId && !currentUserService.IsAdmin)
            throw new ForbiddenException("Only owner or admin can manage access.");
    }
}
