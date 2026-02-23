using InventoryApp.Domain.Enums;

namespace InventoryApp.Domain.Entities;

public class InventoryField
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public FieldType Type { get; set; }
    public bool DisplayInTable { get; set; } = false;
    public int Order { get; set; }
    public int InventoryId { get; set; }
    public virtual Inventory Inventory { get; set; } = null!;
    public ICollection<ItemFieldValue> Values { get; set; } = [];
}
