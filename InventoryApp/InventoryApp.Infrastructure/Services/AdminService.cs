using InventoryApp.Application.DTOs.Admin;
using InventoryApp.Application.Interfaces;
using InventoryApp.Application.Mappers;
using InventoryApp.Domain.Common;
using InventoryApp.Domain.Entities;
using InventoryApp.Domain.Extentions;
using InventoryApp.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryApp.Infrastructure.Services;

internal sealed class AdminService(
    AppDbContext context,
    ICurrentUserService currentUserService,
    ILogger<AdminService> logger) : IAdminService
{
    public async Task<PagedResult<AdminUserDto>> GetUsersAsync(int page, int size, string? search)
    {
        logger.LogInformation("Admin requested user list. Page: {Page}, Size: {Size}, Search: {Search}", page, size, search);

        var query = context.Users
            .Where(x => search == null ||
                   x.DisplayName.Contains(search) ||
                   x.Email.Contains(search));

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(x => x.MapToDto())
            .ToListAsync();

        return new PagedResult<AdminUserDto>
        {
            Items = users,
            Page = page,
            PageSize = size,
            TotalCount = totalCount
        };
    }

    public async Task BlockUserAsync(string userId)
    {
        var user = await GetOrThrowAsync(userId);

        if (user.Id == currentUserService.UserId)
        {
            logger.LogWarning("Admin {AdminId} attempted to block themselves.", currentUserService.UserId);
            throw new DomainException("Admin cannot block himself.");
        }

        user.IsBlocked = true;
        await context.SaveChangesAsync();
        logger.LogInformation("User {UserId} was blocked by Admin {AdminId}.", userId, currentUserService.UserId);
    }

    public async Task UnblockUserAsync(string userId)
    {
        var user = await GetOrThrowAsync(userId);

        user.IsBlocked = false;
        await context.SaveChangesAsync();
        logger.LogInformation("User {UserId} was unblocked by Admin {AdminId}.", userId, currentUserService.UserId);
    }

    public async Task DeleteUserAsync(string userId)
    {
        var user = await GetOrThrowAsync(userId);

        if (user.Id == currentUserService.UserId)
        {
            logger.LogWarning("Admin {AdminId} attempted to delete themselves.", currentUserService.UserId);
            throw new DomainException("Admin cannot delete himself.");
        }

        context.Users.Remove(user);
        await context.SaveChangesAsync();
        logger.LogInformation("User {UserId} was deleted by Admin {AdminId}.", userId, currentUserService.UserId);
    }

    public async Task<AdminStatsDto> GetStatsAsync()
    {
        logger.LogInformation("Admin requested system statistics.");

        var users = await context.Users.Select(x => x.MapToDto()).ToListAsync();
        var inventories = await context.Inventories.Include(x => x.Items).ToListAsync();
        var totalItems = await context.Items.CountAsync();

        var topInventoriesByItems = inventories
            .OrderByDescending(x => x.Items.Count)
            .Take(5)
            .Select(x => x.MapToListItemDto())
            .ToList();

        var recentUsers = users.OrderByDescending(x => x.CreatedAt).Take(5).ToList();

        return new(users.Count, inventories.Count, totalItems, topInventoriesByItems, recentUsers);
    }

    private async Task<User> GetOrThrowAsync(string userId)
    {
        var user = await context.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
        {
            logger.LogWarning("User with ID {UserId} was not found.", userId);
            throw new NotFoundException($"User {userId} not found.");
        }
        return user;
    }
}