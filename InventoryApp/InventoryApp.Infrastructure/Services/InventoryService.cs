using InventoryApp.Application.DTOs.Inventory;
using InventoryApp.Application.Interfaces;
using InventoryApp.Application.Mappers;
using InventoryApp.Domain.Common;
using InventoryApp.Domain.Entities;
using InventoryApp.Domain.Exceptions;
using InventoryApp.Domain.Interfaces;
using InventoryApp.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryApp.Infrastructure.Services;

internal sealed class InventoryService(
    IInventoryRepository repository,
    ICurrentUserService currentUserService,
    AppDbContext context,
    ILogger<InventoryService> logger) : IInventoryService
{
    public async Task<PagedResult<InventoryListItemDto>> GetPagedAsync(int page, int size)
    {
        var userId = currentUserService.UserId;
        var isAdmin = currentUserService.IsAdmin;

        var result = await repository.GetPagedAsync(page, size, userId, isAdmin);

        return new PagedResult<InventoryListItemDto>
        {
            Items = [.. result.Items.Select(i => i.MapToListItemDto())],
            TotalCount = result.TotalCount,
            Page = page,
            PageSize = size
        };
    }

    public async Task<InventoryDto> GetByIdAsync(int id)
    {
        var inventory = await GetOrThrowAsync(id);

        if (!inventory.IsPublic)
        {
            var userId = currentUserService.UserId;
            var hasAccess = currentUserService.IsAdmin
                || inventory.OwnerId == userId
                || inventory.AccessList.Any(x => x.UserId == userId);

            if (!hasAccess)
                throw new ForbiddenException("This inventory is private.");
        }

        return inventory.MapToDto();
    }

    public async Task<InventoryDto> CreateAsync(CreateInventoryDto dto)
    {
        var inventory = dto.MapToEntity();
        inventory.OwnerId = currentUserService.UserId;

        await AttachTagsAsync(inventory, dto.Tags);
        await repository.AddAsync(inventory);

        logger.LogInformation("User {UserId} created inventory {InventoryId} '{InventoryTitle}'.", currentUserService.UserId, inventory.Id, inventory.Title);
        return inventory.MapToDto();
    }

    public async Task<InventoryDto> UpdateAsync(int id, UpdateInventoryDto dto)
    {
        var inventory = await GetOrThrowAsync(id);

        if (inventory.OwnerId != currentUserService.UserId && !currentUserService.IsAdmin)
        {
            logger.LogWarning("User {UserId} attempted to update inventory {InventoryId} without permission.", currentUserService.UserId, id);
            throw new ForbiddenException("You don't have access.");
        }

        inventory.Title = dto.Title;
        inventory.Description = dto.Description;
        inventory.ImageUrl = dto.ImageUrl;
        inventory.IsPublic = dto.IsPublic;
        inventory.CategoryId = dto.CategoryId;
        inventory.UpdatedAt = DateTime.UtcNow;

        inventory.InventoryTags.Clear();
        await AttachTagsAsync(inventory, dto.Tags);

        context.Entry(inventory)
            .Property(i => i.RowVersion)
            .OriginalValue = dto.RowVersion;

        try
        {
            await repository.UpdateAsync(inventory);
            logger.LogInformation("Inventory {InventoryId} updated successfully by user {UserId}.", id, currentUserService.UserId);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning(ex, "Concurrency conflict occurred while updating inventory {InventoryId}.", id);
            throw new OptimisticLockException();
        }

        return inventory.MapToDto();
    }

    public async Task DeleteAsync(int id)
    {
        var inventory = await GetOrThrowAsync(id);

        if (inventory.OwnerId != currentUserService.UserId && !currentUserService.IsAdmin)
        {
            logger.LogWarning("User {UserId} attempted to delete inventory {InventoryId} without permission.", currentUserService.UserId, id);
            throw new ForbiddenException("You don't have access.");
        }

        await repository.DeleteAsync(id);
        logger.LogInformation("Inventory {InventoryId} deleted successfully by user {UserId}.", id, currentUserService.UserId);
    }

    private async Task<Inventory> GetOrThrowAsync(int id)
    {
        var inventory = await repository.GetByIdAsync(id);
        if (inventory == null)
        {
            logger.LogWarning("Inventory {InventoryId} not found.", id);
            throw new NotFoundException($"Inventory {id} not found.");
        }
        return inventory;
    }

    private async Task AttachTagsAsync(Inventory inventory, List<string>? tagNames)
    {
        foreach (var tagName in tagNames ?? [])
        {
            var tag = await context.Tags.FirstOrDefaultAsync(t => t.Name == tagName)
                ?? new Tag { Name = tagName };

            inventory.InventoryTags.Add(new InventoryTag { Tag = tag });
        }
    }
}