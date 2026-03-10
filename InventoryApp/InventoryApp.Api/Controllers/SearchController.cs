using InventoryApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventoryApp.Api.Controllers;

[ApiController]
[Route("/api/search")]
public class SearchController(ISearchService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        var result = await service.SearchAsync(query);

        return Ok(result);
    }
}
