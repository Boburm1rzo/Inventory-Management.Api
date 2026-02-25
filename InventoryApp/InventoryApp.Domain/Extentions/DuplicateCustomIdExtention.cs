namespace InventoryApp.Domain.Extentions;

public class DuplicateCustomIdExtention : DomainException
{
    public string ConflictingId { get; }
    public DuplicateCustomIdExtention(string id) : base($"Custom ID '{id}' already exists in this inventory.")
    {
        ConflictingId = id;
    }
}
