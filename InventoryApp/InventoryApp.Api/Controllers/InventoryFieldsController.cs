using InventoryApp.Application.DTOs.Field;
using InventoryApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryApp.Api.Controllers;

[ApiController]
[Route("api/inventories/{inventoryId:int}/fields")]
public class InventoryFieldsController(IInventoryFieldService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetFields(int inventoryId)
        => Ok(await service.GetFieldsAsync(inventoryId));

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddField(int inventoryId, CreateInventoryFieldDto dto)
        => Ok(await service.AddFieldAsync(inventoryId, dto));

    [HttpPut("{fieldId:int}")]
    [Authorize]
    public async Task<IActionResult> UpdateField(int inventoryId, int fieldId, UpdateInventoryFieldDto dto)
       => Ok(await service.UpdateFieldAsync(inventoryId, fieldId, dto));

    [HttpDelete("{fieldId:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteField(int inventoryId, int fieldId)
    {
        await service.DeleteFieldAsync(inventoryId, fieldId);
        return NoContent();
    }

    [HttpPut("reorder")]
    [Authorize]
    public async Task<IActionResult> ReorderFields(int inventoryId, ReorderFieldsDto dto)
    {
        await service.ReorderFieldsAsync(inventoryId, dto);
        return NoContent();
    }
}
