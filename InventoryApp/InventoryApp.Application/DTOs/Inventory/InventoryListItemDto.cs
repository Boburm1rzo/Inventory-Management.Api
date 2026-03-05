namespace InventoryApp.Application.DTOs.Inventory;

public sealed record InventoryListItemDto(
    int Id,
    string Title,
    string CategoryName,
    string OwnerName,
    DateTime CreatedAt);
