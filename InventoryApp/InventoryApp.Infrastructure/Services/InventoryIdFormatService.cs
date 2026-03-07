using InventoryApp.Application.DTOs.InventoryIdFormat;
using InventoryApp.Application.Interfaces;
using InventoryApp.Application.Mappers;
using InventoryApp.Infrastructure.Helpers;
using InventoryApp.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Infrastructure.Services;

internal sealed class InventoryIdFormatService(
    AppDbContext context,
    InventoryAccessChecker accessChecker,
    ICustomIdGenerator customIdGenerator) : IInventoryIdFormatService
{
    public async Task<List<InventoryIdFormatPartDto>> GetPartsAsync(int inventoryId)
    {
        var parts = await context.InventoryIdFormatParts
            .Where(x => x.InventoryId == inventoryId)
            .OrderBy(x => x.Order)
            .ToListAsync();

        var dtos = parts
            .Select(x => x.MapToDto())
            .ToList();

        return dtos;
    }

    public async Task<InventoryIdFormatPartDto> AddPartAsync(int inventoryId, CreateIdFormatPartDto dto)
    {
        await accessChecker.CheckAsync(inventoryId);

        var order = await context.InventoryIdFormatParts
            .CountAsync(x => x.InventoryId == inventoryId);

        var newIdFormat = dto.MapToEntity();
        newIdFormat.Order = order;

        context.InventoryIdFormatParts.Add(newIdFormat);
        await context.SaveChangesAsync();

        return newIdFormat.MapToDto();
    }

    public async Task DeletePartAsync(int inventoryId, int partId)
    {
        await accessChecker.CheckAsync(inventoryId);

        await context.InventoryIdFormatParts
            .Where(x => x.InventoryId == inventoryId && x.Id == partId)
            .ExecuteDeleteAsync();
    }

    public async Task ReorderPartsAsync(int inventoryId, ReorderIdFormatPartsDto dto)
    {
        await accessChecker.CheckAsync(inventoryId);

        var parts = await context.InventoryIdFormatParts
            .Where(x => x.InventoryId == inventoryId)
            .ToListAsync();

        foreach (var part in parts)
        {
            var newOrder = dto.OrderedIds.IndexOf(part.Id);
            if (newOrder != -1)
                part.Order = newOrder;
        }

        await context.SaveChangesAsync();
    }

    public async Task<string> PreviewIdAsync(int inventoryId)
    {
        var parts = await context.InventoryIdFormatParts
            .Where(x => x.InventoryId == inventoryId)
            .ToListAsync();

        var nextSequence = await context.Items
            .Where(x => x.InventoryId == inventoryId)
            .CountAsync() + 1;

        var preview = customIdGenerator.Generate(parts, nextSequence);

        return preview;
    }
}
