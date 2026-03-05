namespace InventoryApp.Domain.Extentions;

public class ForbiddenException : DomainException
{
    public ForbiddenException(string message) : base(message)
    {

    }
}
