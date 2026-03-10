namespace InventoryApp.Application.DTOs.Post;

public sealed record PostDto(
    int Id,
    string Content,
    string AuthorName,
    string? AvatarUrl,
    DateTime CreatedAt);
