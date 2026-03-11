using InventoryApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryApp.Api.Controllers;

[ApiController]
[Route("api/items/{itemId:int}/likes")]
public class LikesController(ILikeService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetLikeStatus(int itemId)
    {
        var result = await service.GetLikeStatusAsync(itemId);

        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ToggleLike(int itemId)
    {
        var result = await service.ToggleLikeAsync(itemId);

        return Ok(result);
    }
}
