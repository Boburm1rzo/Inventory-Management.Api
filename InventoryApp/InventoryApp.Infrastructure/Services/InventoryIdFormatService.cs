using InventoryApp.Application.DTOs.InventoryIdFormat;
using InventoryApp.Application.Interfaces;
using InventoryApp.Application.Mappers;
using InventoryApp.Infrastructure.Helpers;
using InventoryApp.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryApp.Infrastructure.Services;

internal sealed class InventoryIdFormatService(
    AppDbContext context,
    AccessChecker accessChecker,
    ICustomIdGenerator customIdGenerator,
    ILogger<InventoryIdFormatService> logger) : IInventoryIdFormatService
{
    public async Task<List<InventoryIdFormatPartDto>> GetPartsAsync(int inventoryId)
    {
        var parts = await context.InventoryIdFormatParts
            .Where(x => x.InventoryId == inventoryId)
            .OrderBy(x => x.Order)
            .ToListAsync();

        return parts.Select(x => x.MapToDto()).ToList();
    }

    public async Task<InventoryIdFormatPartDto> AddPartAsync(int inventoryId, CreateIdFormatPartDto dto)
    {
        await accessChecker.CheckInventoryAsync(inventoryId);

        var order = await context.InventoryIdFormatParts
            .CountAsync(x => x.InventoryId == inventoryId);

        var newIdFormat = dto.MapToEntity();
        newIdFormat.Order = order;

        context.InventoryIdFormatParts.Add(newIdFormat);
        await context.SaveChangesAsync();

        logger.LogInformation("Added new ID format part {PartType} to inventory {InventoryId}.", dto.Type, inventoryId);
        return newIdFormat.MapToDto();
    }

    public async Task DeletePartAsync(int inventoryId, int partId)
    {
        await accessChecker.CheckInventoryAsync(inventoryId);

        await context.InventoryIdFormatParts
            .Where(x => x.InventoryId == inventoryId && x.Id == partId)
            .ExecuteDeleteAsync();

        logger.LogInformation("Deleted ID format part {PartId} from inventory {InventoryId}.", partId, inventoryId);
    }

    public async Task ReorderPartsAsync(int inventoryId, ReorderIdFormatPartsDto dto)
    {
        await accessChecker.CheckInventoryAsync(inventoryId);

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
        logger.LogInformation("Reordered ID format parts for inventory {InventoryId}.", inventoryId);
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
        logger.LogInformation("Generated preview ID '{PreviewId}' for inventory {InventoryId}.", preview, inventoryId);

        return preview;
    }
}