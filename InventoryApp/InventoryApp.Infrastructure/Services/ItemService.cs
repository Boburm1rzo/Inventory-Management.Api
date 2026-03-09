using InventoryApp.Application.DTOs.Item;
using InventoryApp.Application.Interfaces;
using InventoryApp.Application.Mappers;
using InventoryApp.Domain.Common;
using InventoryApp.Domain.Entities;
using InventoryApp.Domain.Extentions;
using InventoryApp.Domain.Interfaces;
using InventoryApp.Infrastructure.Helpers;
using InventoryApp.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Infrastructure.Services;

internal sealed class ItemService(
    IItemRepository repository,
    ICurrentUserService currentUserService,
    ItemAccessChecker accessChecker,
    AppDbContext context,
    ICustomIdGenerator customIdGenerator) : IItemService
{
    public async Task<PagedResult<ItemListItemDto>> GetPagedAsync(int inventoryId, int page, int size)
    {
        var items = await repository.GetPagedAsync(inventoryId, page, size);

        return new PagedResult<ItemListItemDto>
        {
            Items = [.. items.Items.Select(x => x.MapToListDto())],
            TotalCount = items.TotalCount,
            Page = page,
            PageSize = size
        };
    }

    public async Task<ItemDto> GetByIdAsync(int inventoryId, int itemId)
    {
        var item = await GetOrThrowAsync(inventoryId, itemId);

        return item.MapToDto();
    }

    public async Task<ItemDto> CreateAsync(int inventoryId, CreateItemDto dto)
    {
        await accessChecker.CheckAsync(inventoryId);

        var parts = await context.InventoryIdFormatParts
                   .Where(x => x.InventoryId == inventoryId)
                   .OrderBy(x => x.Order)
                   .ToListAsync();

        var nextSeq = await context.Items
            .CountAsync(x => x.InventoryId == inventoryId) + 1;

        var item = dto.MapToEntity();

        item.InventoryId = inventoryId;
        item.CreatedById = currentUserService.UserId;
        item.CustomId = customIdGenerator.Generate(parts, nextSeq);

        try
        {
            await repository.AddAsync(item);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException?.Message.Contains("duplicate") == true)
        {
            throw new DuplicateCustomIdExtention(item.CustomId);
        }

        return item.MapToDto();
    }

    public async Task<ItemDto> UpdateAsync(int inventoryId, int itemId, UpdateItemDto dto)
    {
        var item = await GetOrThrowAsync(inventoryId, itemId);

        if (item.CreatedById != currentUserService.UserId && !currentUserService.IsAdmin)
            throw new ForbiddenException("You don't have access to update.");

        item.FieldValues.Clear();
        foreach (var field in dto.FieldValues)
            item.FieldValues.Add(field.MapToEntity());

        context.Entry(item)
            .Property(i => i.RowVersion)
            .OriginalValue = dto.RowVersion;

        try
        {
            await repository.UpdateAsync(item);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new OptimisticLockException();
        }

        return item.MapToDto();
    }

    public async Task DeleteAsync(int inventoryId, int itemId)
    {
        var item = await GetOrThrowAsync(inventoryId, itemId);

        if (item.CreatedById != currentUserService.UserId && !currentUserService.IsAdmin)
            throw new ForbiddenException("You don't have access to delete.");

        await repository.DeleteAsync(inventoryId, itemId);
    }

    private async Task<Item> GetOrThrowAsync(int inventoryId, int itemId)
    {
        var item = await repository.GetByIdAsync(inventoryId, itemId)
            ?? throw new NotFoundException($"Item {itemId} in inventory {inventoryId} not found.");

        return item;
    }
}
