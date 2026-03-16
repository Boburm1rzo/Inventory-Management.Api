using InventoryApp.Application.DTOs.Like;
using InventoryApp.Application.Interfaces;
using InventoryApp.Domain.Exceptions;
using InventoryApp.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryApp.Infrastructure.Services;

internal sealed class LikeService(
    AppDbContext context,
    ICurrentUserService currentUserService,
    ILogger<LikeService> logger) : ILikeService
{
    public async Task<LikeDto> ToggleLikeAsync(int itemId)
    {
        var item = await context.Items.FirstOrDefaultAsync(x => x.Id == itemId);
        if (item == null)
        {
            logger.LogWarning("Attempted to toggle like for non-existent item {ItemId}.", itemId);
            throw new NotFoundException($"Item {itemId} not found.");
        }

        var userId = currentUserService.UserId;

        var existingLike = await context.Likes
            .FirstOrDefaultAsync(x => x.ItemId == itemId && x.UserId == userId);

        if (existingLike is not null)
        {
            context.Likes.Remove(existingLike);
            logger.LogInformation("User {UserId} removed like from item {ItemId}.", userId, itemId);
        }
        else
        {
            context.Likes.Add(new() { ItemId = itemId, UserId = userId });
            logger.LogInformation("User {UserId} added like to item {ItemId}.", userId, itemId);
        }

        await context.SaveChangesAsync();

        var totalLikes = await context.Likes.CountAsync(x => x.ItemId == itemId);

        return new(itemId, existingLike is null, totalLikes);
    }

    public async Task<LikeDto> GetLikeStatusAsync(int itemId)
    {
        var userId = currentUserService.UserId;

        var likeData = await context.Likes
            .Where(x => x.ItemId == itemId)
            .GroupBy(_ => true)
            .Select(g => new
            {
                Total = g.Count(),
                UserLiked = g.Any(x => x.UserId == userId)
            })
            .FirstOrDefaultAsync();

        return new(itemId, likeData?.UserLiked ?? false, likeData?.Total ?? 0);
    }
}