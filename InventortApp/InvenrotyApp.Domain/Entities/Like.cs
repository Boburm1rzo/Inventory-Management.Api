namespace InventoryApp.Domain.Entities;

public class Like
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int ItemId { get; set; }
    public virtual Item Item { get; set; } = null!;
    public string? UserId { get; set; }
    public virtual User User { get; set; } = null!;
}
