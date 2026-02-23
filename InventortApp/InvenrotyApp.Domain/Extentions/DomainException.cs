namespace InventoryApp.Domain.Extentions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
