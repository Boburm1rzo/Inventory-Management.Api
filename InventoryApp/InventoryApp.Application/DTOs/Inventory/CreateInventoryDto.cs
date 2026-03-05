namespace InventoryApp.Application.DTOs.Inventory;

public sealed record CreateInventoryDto(
    string Title,
    string Description,
    int CategoryId,
    string ImageUrl,
    bool IsPublic,
    List<int> tagIds);
