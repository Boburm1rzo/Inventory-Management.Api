using InventoryApp.Application.DTOs.Post;
using InventoryApp.Application.Interfaces;
using InventoryApp.Application.Mappers;
using InventoryApp.Domain.Entities;
using InventoryApp.Domain.Extentions;
using InventoryApp.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Infrastructure.Services;

internal sealed class PostService(
    AppDbContext context,
    ICurrentUserService currentUserService,
    IDiscussionHubService discussionHub) : IPostService
{
    public async Task<List<PostDto>> GetPostsAsync(int inventoryId)
    {
        var posts = await context.Posts
            .Include(x => x.Author)
            .Where(x => x.InventoryId == inventoryId)
            .OrderBy(x => x.CreatedAt)
            .Select(x => x.MapToDto())
            .ToListAsync();

        return posts;
    }

    public async Task<PostDto> CreatePostAsync(int inventoryId, CreatePostDto dto)
    {
        var post = new Post
        {
            Content = dto.Content,
            AuthorId = currentUserService.UserId,
            InventoryId = inventoryId
        };

        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var postDto = post.MapToDto();

        await discussionHub.SendNewPostAsync(inventoryId, postDto);

        return postDto;
    }

    public async Task DeletePostAsync(int inventoryId, int postId)
    {
        var post = await context.Posts
            .FirstOrDefaultAsync(x => x.InventoryId == inventoryId && x.Id == postId)
            ?? throw new NotFoundException($"Post {postId} not found.");

        if (post.AuthorId != currentUserService.UserId && !currentUserService.IsAdmin)
            throw new ForbiddenException("You can only delete your own posts.");

        await context.Posts
            .Where(x => x.Id == postId)
            .ExecuteDeleteAsync();
    }
}
