using InventoryApp.Application.DTOs.Admin;
using InventoryApp.Domain.Entities;

namespace InventoryApp.Application.Mappers;

public static class AdminMappings
{
    public static AdminUserDto MapToDto(this User user) => new(
        user.Id,
        user.DisplayName,
        user.Email,
        user.AvatarUrl,
        user.IsBlocked,
        user.CreatedAt,
        user.OwnedInventories.Count,
        user.Items.Count);
}
