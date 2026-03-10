using InventoryApp.Application.DTOs.Access;
using InventoryApp.Domain.Entities;

namespace InventoryApp.Application.Mappers;

public static class AccessMappings
{
    public static InventoryAccessDto MapToDto(this InventoryAccess access) => new(
        access.UserId,
        access.User.DisplayName,
        access.User.AvatarUrl,
        access.GrantedAt);
}
