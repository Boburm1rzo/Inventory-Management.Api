using InventoryApp.Application.DTOs.InventoryIdFormat;
using InventoryApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryApp.Api.Controllers;

[ApiController]
[Route("api/inventories/{inventoryId:int}/id-format")]
public class InventoryIdFormatController(IInventoryIdFormatService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetParts(int inventoryId)
       => Ok(await service.GetPartsAsync(inventoryId));

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddPart(int inventoryId, CreateIdFormatPartDto dto)
    {
        var result = await service.AddPartAsync(inventoryId, dto);

        return Ok(result);
    }
    [HttpDelete("{partId:int}")]
    [Authorize]
    public async Task<IActionResult> DeletePart(int inventoryId, int partId)
    {
        await service.DeletePartAsync(inventoryId, partId);
        return NoContent();
    }

    [HttpPut("reorder")]
    [Authorize]
    public async Task<IActionResult> ReorderParts(int inventoryId, ReorderIdFormatPartsDto dto)
    {
        await service.ReorderPartsAsync(inventoryId, dto);
        return NoContent();
    }

    [HttpGet("preview")]
    public async Task<IActionResult> Preview(int inventoryId)
        => Ok(await service.PreviewIdAsync(inventoryId));
}
