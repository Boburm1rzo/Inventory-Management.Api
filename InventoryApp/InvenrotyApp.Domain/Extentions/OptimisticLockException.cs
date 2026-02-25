namespace InventoryApp.Domain.Extentions;

public class OptimisticLockException : DomainException
{
    public OptimisticLockException()
        : base("The record was modified by another process. Please reload and try again.")
    {
    }
}
