using InventoryApp.Domain.Enums;

namespace InventoryApp.Domain.Entities;

public class InventoryIdFormatPart
{
    public int Id { get; set; }
    public IdFormatPartType Type { get; set; }
    public int Order { get; set; }
    public string? Config { get; set; }

    public int IventoryId { get; set; }
    public virtual Inventory Inventory { get; set; } = null!;
}
