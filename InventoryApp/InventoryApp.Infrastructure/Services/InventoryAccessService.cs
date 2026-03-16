using InventoryApp.Application.DTOs.Access;
using InventoryApp.Application.DTOs.User;
using InventoryApp.Application.Interfaces;
using InventoryApp.Application.Mappers;
using InventoryApp.Domain.Entities;
using InventoryApp.Domain.Exceptions;
using InventoryApp.Infrastructure.Helpers;
using InventoryApp.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryApp.Infrastructure.Services;

internal sealed class InventoryAccessService(
    AppDbContext context,
    AccessChecker accessChecker,
    ILogger<InventoryAccessService> logger) : IInventoryAccessService
{
    public async Task<List<InventoryAccessDto>> GetAccessListAsync(int inventoryId)
    {
        logger.LogInformation("Fetching access list for inventory {InventoryId}.", inventoryId);

        var accessLists = await context.InventoryAccesses
            .Include(x => x.User)
            .Where(x => x.InventoryId == inventoryId)
            .ToListAsync();

        return accessLists.Select(x => x.MapToDto()).ToList();
    }

    public async Task AddAccessAsync(int inventoryId, AddAccessDto dto)
    {
        await accessChecker.CheckOwnerAsync(inventoryId);

        var existingAccess = await context.InventoryAccesses
            .AnyAsync(x => x.InventoryId == inventoryId && x.UserId == dto.UserId);

        if (existingAccess)
        {
            logger.LogWarning("Attempted to add access for user {UserId} to inventory {InventoryId}, but user already has access.", dto.UserId, inventoryId);
            throw new DomainException("User already has access.");
        }

        var newAccess = new InventoryAccess
        {
            GrantedAt = DateTime.UtcNow,
            InventoryId = inventoryId,
            UserId = dto.UserId,
        };

        context.InventoryAccesses.Add(newAccess);
        await context.SaveChangesAsync();
        logger.LogInformation("Granted access to user {UserId} for inventory {InventoryId}.", dto.UserId, inventoryId);
    }

    public async Task RemoveAccessAsync(int inventoryId, string userId)
    {
        await accessChecker.CheckOwnerAsync(inventoryId);

        var inventory = await context.Inventories.FirstOrDefaultAsync(x => x.Id == inventoryId);
        if (inventory == null)
        {
            throw new NotFoundException($"Inventory {inventoryId} not found.");
        }

        if (inventory.OwnerId == userId)
        {
            logger.LogWarning("Attempted to remove owner {UserId} from inventory {InventoryId} access list.", userId, inventoryId);
            throw new ForbiddenException("Cannot remove owner from access list.");
        }

        await context.InventoryAccesses
            .Where(x => x.InventoryId == inventoryId && x.UserId == userId)
            .ExecuteDeleteAsync();

        logger.LogInformation("Removed access for user {UserId} from inventory {InventoryId}.", userId, inventoryId);
    }

    public async Task<List<UserSearchDto>> SearchUsersAsync(string query)
    {
        logger.LogInformation("Searching users with query: {Query}", query);
        return await context.Users
            .Where(x => x.DisplayName.Contains(query) || x.Email.Contains(query))
            .Take(10)
            .Select(x => new UserSearchDto(x.Id, x.DisplayName, x.Email, x.AvatarUrl))
            .ToListAsync();
    }
}