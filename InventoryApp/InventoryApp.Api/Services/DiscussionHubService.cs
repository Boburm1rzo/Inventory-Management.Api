using InventoryApp.Api.Hubs;
using InventoryApp.Application.DTOs.Post;
using InventoryApp.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace InventoryApp.Api.Services;

public class DiscussionHubService(IHubContext<DiscussionHub> hubContext) : IDiscussionHubService
{
    public async Task SendNewPostAsync(int inventoryId, PostDto post)
        => await hubContext.Clients
            .Group(inventoryId.ToString())
            .SendAsync("NewPost", post);
}
