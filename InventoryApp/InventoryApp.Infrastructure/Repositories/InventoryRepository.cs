using InventoryApp.Domain.Common;
using InventoryApp.Domain.Entities;
using InventoryApp.Domain.Interfaces;
using InventoryApp.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Infrastructure.Repositories;

internal sealed class InventoryRepository(AppDbContext context) : IInventoryRepository
{
    public async Task<PagedResult<Inventory>> GetPagedAsync(int page, int size)
    {
        var query = context.Inventories
            .Include(x => x.Owner)
            .Include(x => x.Category)
            .AsNoTracking();

        var total = await query.CountAsync();

        var inventories = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return new()
        {
            Items = inventories,
            Page = page,
            PageSize = size,
            TotalCount = total
        };
    }

    public async Task<Inventory?> GetByIdAsync(int id)
        => await context.Inventories
            .Include(x => x.Owner)
            .Include(x => x.Category)
            .Include(x => x.Fields)
            .Include(x => x.InventoryTags)
                .ThenInclude(t => t.Tag)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task AddAsync(Inventory inventory)
    {
        await context.Inventories.AddAsync(inventory);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Inventory inventory)
    {
        context.Inventories.Update(inventory);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await context.Inventories
            .Where(x => x.Id == id)
            .ExecuteDeleteAsync();
    }
}
