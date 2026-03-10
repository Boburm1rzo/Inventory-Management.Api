using InventoryApp.Application.DTOs.Post;
using InventoryApp.Domain.Entities;

namespace InventoryApp.Application.Mappers;

public static class PostMappings
{
    public static PostDto MapToDto(this Post post) => new(
        post.Id,
        post.Content,
        post.Author.DisplayName,
        post.Author.AvatarUrl,
        post.CreatedAt);
}
