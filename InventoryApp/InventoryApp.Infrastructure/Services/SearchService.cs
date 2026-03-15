using InventoryApp.Application.DTOs.Search;
using InventoryApp.Application.Interfaces;
using InventoryApp.Application.Mappers;
using InventoryApp.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryApp.Infrastructure.Services;

internal sealed class SearchService(
    AppDbContext context,
    ILogger<SearchService> logger) : ISearchService
{
    public async Task<SearchResultDto> SearchAsync(string query)
    {
        logger.LogInformation("Global search performed with query: '{Query}'", query);

        var inventories = await context.Inventories
            .Where(i => i.IsPublic && (
                i.Title.Contains(query) ||
                (i.Description != null && i.Description.Contains(query))))
            .Include(i => i.Owner)
            .Include(i => i.Category)
            .Take(20)
            .ToListAsync();

        var itemValues = await context.ItemFieldValues
            .Where(v => v.TextValue != null && v.TextValue.Contains(query))
            .Include(v => v.Item)
                .ThenInclude(i => i.Inventory)
            .Include(v => v.Field)
            .Take(20)
            .ToListAsync();

        var items = itemValues
            .GroupBy(v => v.ItemId)
            .Select(g => new ItemSearchResultDto(
                g.First().Item.Id,
                g.First().Item.CustomId,
                g.First().Item.InventoryId,
                g.First().Item.Inventory.Title,
                g.Select(v => v.Field.Title).ToList()
            ))
            .ToList();

        return new SearchResultDto(
            inventories.Select(i => i.MapToListItemDto()).ToList(),
            items
        );
    }
}