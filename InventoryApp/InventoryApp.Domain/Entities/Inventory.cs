using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Domain.Entities;

public class Inventory
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;

    public string OwnerId { get; set; } = string.Empty;
    public virtual User Owner { get; set; } = null!;
    public int? CategoryId { get; set; }
    public virtual Category? Category { get; set; }

    public ICollection<InventoryField> Fields { get; set; } = [];
    public ICollection<InventoryIdFormatPart> IdFormatParts { get; set; } = [];
    public ICollection<InventoryAccess> AccessList { get; set; } = [];
    public ICollection<InventoryTag> InventoryTags { get; set; } = [];
    public ICollection<Item> Items { get; set; } = [];
    public ICollection<Post> Posts { get; set; } = [];
}