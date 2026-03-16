using InventoryApp.Application.DTOs.Category;
using InventoryApp.Application.Interfaces;
using InventoryApp.Application.Mappers;
using InventoryApp.Domain.Entities;
using InventoryApp.Domain.Exceptions;
using InventoryApp.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryApp.Infrastructure.Services;

internal sealed class CategoryService(
    AppDbContext context,
    ILogger<CategoryService> logger) : ICategoryService
{
    public async Task<List<CategoryDto>> GetAllAsync()
    {
        logger.LogInformation("Fetching all categories.");
        return await context.Categories.Select(x => x.MapToDto()).ToListAsync();
    }

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

        logger.LogInformation("Category '{CategoryName}' created with ID {CategoryId}.", category.Name, category.Id);
        return category.MapToDto();
    }

    public async Task<CategoryDto> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var category = await GetOrThrowAsync(id);
        category.Name = dto.Name;
        await context.SaveChangesAsync();

        logger.LogInformation("Category {CategoryId} updated successfully.", id);
        return category.MapToDto();
    }

    public async Task DeleteAsync(int id)
    {
        var category = await GetOrThrowAsync(id);
        context.Categories.Remove(category);
        await context.SaveChangesAsync();

        logger.LogInformation("Category {CategoryId} deleted successfully.", id);
    }

    private async Task<Category> GetOrThrowAsync(int id)
    {
        var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);
        if (category == null)
        {
            logger.LogWarning("Category with ID {CategoryId} was not found.", id);
            throw new NotFoundException($"Category {id} not found.");
        }
        return category;
    }
}