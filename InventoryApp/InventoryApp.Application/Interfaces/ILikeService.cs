using InventoryApp.Application.DTOs.Like;

namespace InventoryApp.Application.Interfaces;

public interface ILikeService
{
    Task<LikeDto> ToggleLikeAsync(int itemId);
    Task<LikeDto> GetLikeStatusAsync(int itemId);
}
