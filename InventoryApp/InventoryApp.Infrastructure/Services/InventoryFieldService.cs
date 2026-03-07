using InventoryApp.Application.DTOs.Field;
using InventoryApp.Application.Interfaces;
using InventoryApp.Application.Mappers;
using InventoryApp.Domain.Extentions;
using InventoryApp.Infrastructure.Helpers;
using InventoryApp.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Infrastructure.Services;

internal sealed class InventoryFieldService(
    AppDbContext context,
    InventoryAccessChecker accessChecker) : IInventoryFieldService
{
    public async Task<List<InventoryFieldDto>> GetFieldsAsync(int inventoryId)
    {
        var fields = await context.InventoryFields
            .Where(x => x.InventoryId == inventoryId)
            .OrderBy(x => x.Order)
            .ToListAsync();

        var dtos = fields.Select(x => x.MapToDto()).ToList();

        return dtos;
    }

    public async Task<InventoryFieldDto> AddFieldAsync(int inventoryId, CreateInventoryFieldDto dto)
    {
        await accessChecker.CheckAsync(inventoryId);

        var allFields = await context.InventoryFields
            .Where(x => x.InventoryId == inventoryId)
            .Select(x => x.Type)
            .ToListAsync();

        if (allFields.Count(x => x == dto.Type) >= 3)
            throw new DomainException($"Maximum 3 fields of type '{dto.Type}' allowed.");

        var order = allFields.Count;

        var inventoryField = dto.MapToEntity();
        inventoryField.Order = order;

        context.InventoryFields.Add(inventoryField);
        await context.SaveChangesAsync();

        return inventoryField.MapToDto();
    }

    public async Task DeleteFieldAsync(int inventoryId, int fieldId)
    {
        await accessChecker.CheckAsync(inventoryId);

        await context.InventoryFields
            .Where(x => x.Id == fieldId && x.InventoryId == inventoryId)
            .ExecuteDeleteAsync();
    }

    public async Task ReorderFieldsAsync(int inventoryId, ReorderFieldsDto dto)
    {
        await accessChecker.CheckAsync(inventoryId);

        var fields = await context.InventoryFields
            .Where(x => x.InventoryId == inventoryId)
            .ToListAsync();

        foreach (var field in fields)
        {
            var newOrder = dto.OrderedIds.IndexOf(field.Id);
            if (newOrder != -1)
                field.Order = newOrder;
        }

        await context.SaveChangesAsync();
    }

    public async Task<InventoryFieldDto> UpdateFieldAsync(int inventoryId, int fieldId, UpdateInventoryFieldDto dto)
    {
        await accessChecker.CheckAsync(inventoryId);

        var field = await context.InventoryFields
            .FirstOrDefaultAsync(x => x.Id == fieldId && x.InventoryId == inventoryId)
            ?? throw new NotFoundException($"Inventory field {fieldId} not found");

        field.Title = dto.Title;
        field.Description = dto.Description;
        field.DisplayInTable = dto.DisplatInTable;

        await context.SaveChangesAsync();

        return field.MapToDto();
    }
}
