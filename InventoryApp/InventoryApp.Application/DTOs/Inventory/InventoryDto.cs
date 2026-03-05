namespace InventoryApp.Application.DTOs.Inventory;

public sealed record InventoryDto(
    int Id,
    string Title,
    string? Description,
    string? ImageUrl,
    bool IsPublic,
    string OwnerId,
    string OwnerName,
    int? CategoryId,
    string? CategoryName,
    List<string> Tags,
    DateTime CreatedAt,
    byte[] rowVersion);
