using InventoryApp.Application.DTOs.Post;
using InventoryApp.Application.Interfaces;
using InventoryApp.Application.Mappers;
using InventoryApp.Domain.Entities;
using InventoryApp.Domain.Exceptions;
using InventoryApp.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryApp.Infrastructure.Services;

internal sealed class PostService(
    AppDbContext context,
    ICurrentUserService currentUserService,
    IDiscussionHubService discussionHub,
    ILogger<PostService> logger) : IPostService
{
    public async Task<List<PostDto>> GetPostsAsync(int inventoryId)
    {
        return await context.Posts
            .Include(x => x.Author)
            .Where(x => x.InventoryId == inventoryId)
            .OrderBy(x => x.CreatedAt)
            .Select(x => x.MapToDto())
            .ToListAsync();
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

        await context.Entry(post)
            .Reference(p => p.Author)
            .LoadAsync();

        logger.LogInformation("User {UserId} created post {PostId} in inventory {InventoryId}.", currentUserService.UserId, post.Id, inventoryId);

        var postDto = post.MapToDto();
        await discussionHub.SendNewPostAsync(inventoryId, postDto);

        return postDto;
    }

    public async Task DeletePostAsync(int inventoryId, int postId)
    {
        var post = await context.Posts
            .FirstOrDefaultAsync(x => x.InventoryId == inventoryId && x.Id == postId);

        if (post == null)
        {
            logger.LogWarning("Post {PostId} in inventory {InventoryId} not found.", postId, inventoryId);
            throw new NotFoundException($"Post {postId} not found.");
        }

        if (post.AuthorId != currentUserService.UserId && !currentUserService.IsAdmin)
        {
            logger.LogWarning("User {UserId} attempted to delete post {PostId} without permission.", currentUserService.UserId, postId);
            throw new ForbiddenException("You can only delete your own posts.");
        }

        await context.Posts
            .Where(x => x.Id == postId)
            .ExecuteDeleteAsync();

        logger.LogInformation("Post {PostId} deleted successfully from inventory {InventoryId}.", postId, inventoryId);
    }
}