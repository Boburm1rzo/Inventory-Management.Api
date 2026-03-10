using Microsoft.AspNetCore.SignalR;

namespace InventoryApp.Api.Hubs;

public class DiscussionHub : Hub
{
    public async Task JoinInventory(string inventoryId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, inventoryId);

    public async Task LeaveInventory(string inventoryId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, inventoryId);
}
