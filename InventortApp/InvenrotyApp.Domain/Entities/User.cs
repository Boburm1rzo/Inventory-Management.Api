using Microsoft.AspNetCore.Identity;

namespace InventoryApp.Domain.Entities;

public class User : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public bool IsBlocked { get; set; } = false;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Inventory> OwnedInventories { get; set; } = [];
    public ICollection<InventoryAccess> AccessList { get; set; } = [];
    public ICollection<Item> Items { get; set; } = [];
    public ICollection<Post> Posts { get; set; } = [];
    public ICollection<Like> Likes { get; set; } = [];
}
