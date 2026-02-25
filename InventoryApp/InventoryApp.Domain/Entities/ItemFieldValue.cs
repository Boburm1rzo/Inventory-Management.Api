namespace InventoryApp.Domain.Entities;

public class ItemFieldValue
{
    public int Id { get; set; }
    public string? TextValue { get; set; }
    public decimal? NumericValue { get; set; }
    public bool? BooleanValue { get; set; }

    public int ItemId { get; set; }
    public virtual Item Item { get; set; } = null!;
    public int FieldId { get; set; }
    public virtual InventoryField Field { get; set; } = null!;
}
