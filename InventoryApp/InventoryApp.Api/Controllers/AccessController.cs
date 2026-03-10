using InventoryApp.Application.DTOs.Access;
using InventoryApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryApp.Api.Controllers;

[ApiController]
[Route("/api/inventories/{inventoryId:int}/access")]
public class AccessController(IInventoryAccessService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAccessList(int inventoryId)
    {
        var result = await service.GetAccessListAsync(inventoryId);

        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddAccess(int inventoryId, AddAccessDto dto)
    {
        await service.AddAccessAsync(inventoryId, dto);

        return NoContent();
    }

    [HttpDelete("{userId}")]
    [Authorize]
    public async Task<IActionResult> Delete(int inventoryId, string userId)
    {
        await service.RemoveAccessAsync(inventoryId, userId);

        return NoContent();
    }
}
