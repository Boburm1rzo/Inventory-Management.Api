using InventoryApp.Domain.Entities;

namespace InventoryApp.Application.Interfaces;

public interface ICustomIdGenerator
{
    string Generate(List<InventoryIdFormatPart> parts, int nextSequence);
}
