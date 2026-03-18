using InventoryApp.Application.DTOs.Item;
using InventoryApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryApp.Api.Controllers;

[ApiController]
[Route("api/inventories/{inventoryId:int}/items")]
public class ItemsController(IItemService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        int inventoryId,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        var result = await service.GetPagedAsync(inventoryId, page, size);

        return Ok(result);
    }

    [HttpGet("{itemId:int}")]
    public async Task<IActionResult> GetById(int inventoryId, int itemId)
    {
        var result = await service.GetByIdAsync(inventoryId, itemId);

        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(int inventoryId, CreateItemDto dto)
    {
        var result = await service.CreateAsync(inventoryId, dto);

        return Ok(result);
    }

    [HttpPut("{itemId:int}")]
    [Authorize]
    public async Task<IActionResult> Update(int inventoryId, int itemId, UpdateItemDto dto)
    {
        var result = await service.UpdateAsync(inventoryId, itemId, dto);

        return Ok(result);
    }

    [HttpDelete("{itemId:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int inventoryId, int itemId)
    {
        await service.DeleteAsync(inventoryId, itemId);
        return NoContent();
    }
}
