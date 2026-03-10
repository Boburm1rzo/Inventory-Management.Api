using InventoryApp.Application.DTOs.Post;
using InventoryApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryApp.Api.Controllers;

[ApiController]
[Route("api/inventories/{inventoryId:int}/posts")]
public class PostsController(IPostService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(int inventoryId)
    {
        var result = await service.GetPostsAsync(inventoryId);

        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(int inventoryId, CreatePostDto dto)
    {
        var result = await service.CreatePostAsync(inventoryId, dto);

        return Ok(result);
    }

    [HttpDelete("{postId:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int inventoryId, int postId)
    {
        await service.DeletePostAsync(inventoryId, postId);

        return NoContent();
    }
}
