namespace InventoryApp.Application.DTOs.Like;

public sealed record LikeDto(
    int ItemId,
    bool LikedByCurrentUser,
    int TotalLikes);
