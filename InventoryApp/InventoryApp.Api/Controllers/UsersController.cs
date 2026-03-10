using InventoryApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventoryApp.Api.Controllers;

[ApiController]
[Route("/api/users")]
public class UsersController(IInventoryAccessService inventoryAccessService) : ControllerBase
{
    [HttpGet("search")]
    public async Task<IActionResult> SearchUsers([FromQuery] string query)
    {
        var results = await inventoryAccessService.SearchUsersAsync(query);
        return Ok(results);
    }
}
