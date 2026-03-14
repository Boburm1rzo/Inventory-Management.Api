using InventoryApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventoryApp.Api.Controllers;

[ApiController]
[Route("api/tags")]
public class TagsController(ITagService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? prefix = null)
        => Ok(await service.GetAsync(prefix));
}
