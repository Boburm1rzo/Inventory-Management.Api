using InventoryApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryApp.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin")]
public class AdminController(IAdminService service) : ControllerBase
{
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await service.GetStatsAsync();

        return Ok(result);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] string search = null)
    {
        var result = await service.GetUsersAsync(page, size, search);

        return Ok(result);
    }

    [HttpPost("users/{userId}/block")]
    public async Task<IActionResult> BlockUser(string userId)
    {
        await service.BlockUserAsync(userId);

        return NoContent();
    }

    [HttpPost("users/{userId}/unblock")]
    public async Task<IActionResult> UnblockUser(string userId)
    {
        await service.UnblockUserAsync(userId);

        return NoContent();
    }

    [HttpDelete("users/{userId}")]
    public async Task<IActionResult> Delete(string userId)
    {
        await service.DeleteUserAsync(userId);

        return NoContent();
    }
}
