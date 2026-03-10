using InventoryApp.Application.DTOs.Post;

namespace InventoryApp.Application.Interfaces;

public interface IPostService
{
    Task<List<PostDto>> GetPostsAsync(int inventoryId);
    Task<PostDto> CreatePostAsync(int inventoryId, CreatePostDto dto);
    Task DeletePostAsync(int inventoryId, int postId);
}
