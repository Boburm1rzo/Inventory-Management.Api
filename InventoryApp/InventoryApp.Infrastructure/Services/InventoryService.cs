using InventoryApp.Application.DTOs.Inventory;
using InventoryApp.Application.Interfaces;
using InventoryApp.Application.Mappers;
using InventoryApp.Domain.Common;
using InventoryApp.Domain.Extentions;
using InventoryApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Infrastructure.Services;

internal sealed class InventoryService(
    IInventoryRepository repository,
    ICurrentUserService currentUserService) : IInventoryService
{
    public async Task<PagedResult<InventoryListItemDto>> GetPagedAsync(int page, int size)
    {
        var result = await repository.GetPagedAsync(page, size);

        return new PagedResult<InventoryListItemDto>
        {
            Items = result.Items.Select(i => i.MapToListItemDto()).ToList(),
            TotalCount = result.TotalCount,
            Page = page,
            PageSize = size
        };
    }

    public async Task<InventoryDto> GetByIdAsync(int id)
    {
        var inventory = await repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Inventory {id} not found.");

        return inventory.MapToDto();
    }

    public async Task<InventoryDto> CreateAsync(CreateInventoryDto dto)
    {
        var inventory = dto.MapToEntity();
        inventory.OwnerId = currentUserService.UserId;

        await repository.AddAsync(inventory);

        return inventory.MapToDto();
    }

    public async Task<InventoryDto> UpdateAsync(int id, UpdateInventoryDto dto)
    {
        var inventory = await repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Inventory {id} not found.");

        if (inventory.OwnerId != currentUserService.UserId && !currentUserService.IsAdmin)
            throw new ForbiddenException("You don't have access.");

        inventory.Title = dto.Title;
        inventory.Description = dto.Description;
        inventory.ImageUrl = dto.ImageUrl;
        inventory.IsPublic = dto.IsPublic;
        inventory.CategoryId = dto.CategoryId;
        inventory.UpdatedAt = DateTime.UtcNow;

        try
        {
            await repository.UpdateAsync(inventory);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new OptimisticLockException();
        }

        return inventory.MapToDto();
    }

    public async Task DeleteAsync(int id)
    {
        var inventory = await repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Inventory {id} not found.");

        if (inventory.OwnerId != currentUserService.UserId && !currentUserService.IsAdmin)
            throw new ForbiddenException("You don't have access.");

        await repository.DeleteAsync(id);
    }
}
