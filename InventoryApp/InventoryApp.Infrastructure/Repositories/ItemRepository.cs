using InventoryApp.Domain.Common;
using InventoryApp.Domain.Entities;
using InventoryApp.Domain.Interfaces;
using InventoryApp.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Infrastructure.Repositories;

internal sealed class ItemRepository(AppDbContext context) : IItemRepository
{
    public async Task<PagedResult<Item>> GetPagedAsync(int inventoryId, int page, int size)
    {
        var query = context.Items
            .Include(x => x.CreatedBy)
            .Include(x => x.FieldValues)
                .ThenInclude(f => f.Field)
            .AsNoTracking();

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return new()
        {
            Items = items,
            Page = page,
            PageSize = size,
            TotalCount = total
        };
    }

    public async Task<Item?> GetByIdAsync(int inventoryId, int itemId)
        => await context.Items
            .Include(x => x.CreatedBy)
            .Include(x => x.FieldValues)
                .ThenInclude(f => f.Field)
            .FirstOrDefaultAsync(x => x.InventoryId == inventoryId && x.Id == itemId);

    public async Task AddAsync(Item item)
    {
        context.Items.Add(item);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Item item)
    {
        context.Items.Update(item);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int inventoryId, int itemId)
    {
        await context.Items
            .Where(x => x.Id == itemId && x.InventoryId == inventoryId)
            .ExecuteDeleteAsync();
    }
}
