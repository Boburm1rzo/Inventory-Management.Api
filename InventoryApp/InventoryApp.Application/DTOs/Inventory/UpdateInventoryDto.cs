namespace InventoryApp.Application.DTOs.Inventory;

public sealed record UpdateInventoryDto(
    string Title,
    string Description,
    int CategoryId,
    string ImageUrl,
    bool IsPublic,
    List<string> tags,
    byte[] RowVersion);
