using InventoryApp.Application.DTOs.Field;
using InventoryApp.Application.Interfaces;
using InventoryApp.Application.Mappers;
using InventoryApp.Domain.Exceptions;
using InventoryApp.Infrastructure.Helpers;
using InventoryApp.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryApp.Infrastructure.Services;

internal sealed class InventoryFieldService(
    AppDbContext context,
    AccessChecker accessChecker,
    ILogger<InventoryFieldService> logger) : IInventoryFieldService
{
    public async Task<List<InventoryFieldDto>> GetFieldsAsync(int inventoryId)
    {
        var fields = await context.InventoryFields
            .Where(x => x.InventoryId == inventoryId)
            .OrderBy(x => x.Order)
            .ToListAsync();

        return fields.Select(x => x.MapToDto()).ToList();
    }

    public async Task<InventoryFieldDto> AddFieldAsync(int inventoryId, CreateInventoryFieldDto dto)
    {
        await accessChecker.CheckInventoryAsync(inventoryId);

        var allFields = await context.InventoryFields
            .Where(x => x.InventoryId == inventoryId)
            .Select(x => x.Type)
            .ToListAsync();

        if (allFields.Count(x => x == dto.Type) >= 3)
        {
            logger.LogWarning("Inventory {InventoryId} reached maximum fields of type '{Type}'.", inventoryId, dto.Type);
            throw new DomainException($"Maximum 3 fields of type '{dto.Type}' allowed.");
        }

        var inventoryField = dto.MapToEntity();
        inventoryField.Order = allFields.Count;
        inventoryField.InventoryId = inventoryId;

        context.InventoryFields.Add(inventoryField);
        await context.SaveChangesAsync();

        logger.LogInformation("Added new field '{FieldTitle}' of type {FieldType} to inventory {InventoryId}.", dto.Title, dto.Type, inventoryId);
        return inventoryField.MapToDto();
    }

    public async Task DeleteFieldAsync(int inventoryId, int fieldId)
    {
        await accessChecker.CheckInventoryAsync(inventoryId);

        var rowsAffected = await context.InventoryFields
            .Where(x => x.Id == fieldId && x.InventoryId == inventoryId)
            .ExecuteDeleteAsync();

        if (rowsAffected > 0)
            logger.LogInformation("Deleted field {FieldId} from inventory {InventoryId}.", fieldId, inventoryId);
    }

    public async Task ReorderFieldsAsync(int inventoryId, ReorderFieldsDto dto)
    {
        await accessChecker.CheckInventoryAsync(inventoryId);

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
        logger.LogInformation("Reordered fields for inventory {InventoryId}.", inventoryId);
    }

    public async Task<InventoryFieldDto> UpdateFieldAsync(int inventoryId, int fieldId, UpdateInventoryFieldDto dto)
    {
        await accessChecker.CheckInventoryAsync(inventoryId);

        var field = await context.InventoryFields
            .FirstOrDefaultAsync(x => x.Id == fieldId && x.InventoryId == inventoryId);

        if (field == null)
        {
            logger.LogWarning("Field {FieldId} not found in inventory {InventoryId} for update.", fieldId, inventoryId);
            throw new NotFoundException($"Inventory field {fieldId} not found");
        }

        field.Title = dto.Title;
        field.Description = dto.Description;
        field.DisplayInTable = dto.DisplatInTable;

        await context.SaveChangesAsync();
        logger.LogInformation("Updated field {FieldId} in inventory {InventoryId}.", fieldId, inventoryId);

        return field.MapToDto();
    }
}