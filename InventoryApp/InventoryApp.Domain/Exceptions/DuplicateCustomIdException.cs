namespace InventoryApp.Domain.Exceptions;

public class DuplicateCustomIdException : DomainException
{
    public string ConflictingId { get; }
    public DuplicateCustomIdException(string id) : base($"Custom ID '{id}' already exists in this inventory.")
    {
        ConflictingId = id;
    }
}
