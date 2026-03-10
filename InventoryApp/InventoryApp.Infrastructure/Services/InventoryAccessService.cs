using InventoryApp.Application.DTOs.Access;
using InventoryApp.Application.DTOs.User;
using InventoryApp.Application.Interfaces;
using InventoryApp.Application.Mappers;
using InventoryApp.Domain.Entities;
using InventoryApp.Domain.Extentions;
using InventoryApp.Infrastructure.Helpers;
using InventoryApp.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Infrastructure.Services;

internal sealed class InventoryAccessService(
    AppDbContext context,
    AccessChecker accessChecker) : IInventoryAccessService
{
    public async Task<List<InventoryAccessDto>> GetAccessListAsync(int inventoryId)
    {
        var accessLists = await context.InventoryAccesses
            .Include(x => x.User)
            .Where(x => x.InventoryId == inventoryId)
            .ToListAsync();

        var dtos = accessLists.Select(x => x.MapToDto()).ToList();

        return dtos;
    }

    public async Task AddAccessAsync(int inventoryId, AddAccessDto dto)
    {
        await accessChecker.CheckOwnerAsync(inventoryId);

        var user = await context.Users
            .FirstOrDefaultAsync(x => x.Id == dto.UserId);

        if (user is not null)
        {
            throw new DomainException("User allready has access.");
        }

        var newAccess = new InventoryAccess
        {
            GrantedAt = DateTime.UtcNow,
            InventoryId = inventoryId,
            UserId = dto.UserId,
        };

        context.InventoryAccesses.Add(newAccess);
        await context.SaveChangesAsync();
    }

    public async Task RemoveAccessAsync(int inventoryId, string userId)
    {
        await accessChecker.CheckOwnerAsync(inventoryId);

        var inventory = await context.Inventories
            .FirstOrDefaultAsync(x => x.Id == inventoryId)
            ?? throw new NotFoundException($"Inventory {inventoryId} not found.");

        if (inventory.OwnerId == userId)
            throw new ForbiddenException("Cannot remove owner from access list.");

        await context.InventoryAccesses
            .Where(x => x.InventoryId == inventoryId && x.UserId == userId)
            .ExecuteDeleteAsync();
    }

    public async Task<List<UserSearchDto>> SearchUsersAsync(string query)
    {
        var users = await context.Users
            .Where(x => x.DisplayName.Contains(query) || x.Email.Contains(query))
            .Take(10)
            .Select(x => new UserSearchDto(x.Id, x.DisplayName, x.Email, x.AvatarUrl))
            .ToListAsync();

        return users;
    }
}
