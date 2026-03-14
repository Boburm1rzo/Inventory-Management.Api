using InventoryApp.Application.DTOs.Category;
using InventoryApp.Application.Interfaces;
using InventoryApp.Application.Mappers;
using InventoryApp.Domain.Entities;
using InventoryApp.Domain.Extentions;
using InventoryApp.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Infrastructure.Services;

internal sealed class CategoryService(AppDbContext context) : ICategoryService
{
    public async Task<List<CategoryDto>> GetAllAsync()
        => await context.Categories
            .Select(x => x.MapToDto())
            .ToListAsync();

    public async Task<CategoryDto> GetByIdAsync(int id)
    {
        var category = await GetOrThrowAsync(id);

        return category.MapToDto();
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        var category = dto.MapToEntity();

        context.Categories.Add(category);

        await context.SaveChangesAsync();

        return category.MapToDto();
    }

    public async Task<CategoryDto> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var category = await GetOrThrowAsync(id);

        category.Name = dto.Name;

        await context.SaveChangesAsync();

        return category.MapToDto();
    }
    public async Task DeleteAsync(int id)
    {
        var category = await GetOrThrowAsync(id);

        context.Categories.Remove(category);

        await context.SaveChangesAsync();
    }

    private async Task<Category> GetOrThrowAsync(int id)
    {
        var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException($"Category {id} not found.");

        return category;
    }
}
