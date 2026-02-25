namespace InventoryApp.Domain.Entities;

public class InventoryAccess
{
    public int Id { get; set; }
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

    public int InventoryId { get; set; }
    public virtual Inventory Inventory { get; set; } = null!;
    public string? UserId { get; set; }
    public virtual User User { get; set; } = null!;
}
