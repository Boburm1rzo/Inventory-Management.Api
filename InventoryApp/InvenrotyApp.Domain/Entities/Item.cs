using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Domain.Entities;

public class Item
{
    public int Id { get; set; }
    public string CustomId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;

    public int InventoryId { get; set; }
    public virtual Inventory Inventory { get; set; } = null!;
    public string? CreatedById { get; set; }
    public virtual User CreatedBy { get; set; } = null!;
    public ICollection<ItemFieldValue> FieldValues { get; set; } = [];
    public ICollection<Like> Likes { get; set; } = [];
}
