using InventoryApp.Application.DTOs.Post;

namespace InventoryApp.Application.Interfaces;

public interface IDiscussionHubService
{
    Task SendNewPostAsync(int inventoryId, PostDto post);
}
