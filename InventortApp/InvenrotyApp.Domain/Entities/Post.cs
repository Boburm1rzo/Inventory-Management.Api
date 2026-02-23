namespace InventoryApp.Domain.Entities;

public class Post
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int InventoryId { get; set; }
    public virtual Inventory Inventory { get; set; } = null!;
    public string? AuthorId { get; set; }
    public virtual User Author { get; set; } = null!;
}
