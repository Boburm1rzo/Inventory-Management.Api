namespace InventoryApp.Domain.Entities;

public class InventoryTag
{
    public int InventoryId { get; set; }
    public virtual Inventory Inventory { get; set; } = null!;
    public int TagId { get; set; }
    public virtual Tag Tag { get; set; } = null!;
}
