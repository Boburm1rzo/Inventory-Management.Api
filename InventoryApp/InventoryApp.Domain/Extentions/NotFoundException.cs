namespace InventoryApp.Domain.Extentions;

public class NotFoundException : DomainException
{
    public NotFoundException(string message) : base(message)
    {

    }
}
