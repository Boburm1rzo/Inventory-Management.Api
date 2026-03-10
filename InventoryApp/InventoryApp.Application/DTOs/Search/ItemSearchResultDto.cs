namespace InventoryApp.Application.DTOs.Search;

public sealed record ItemSearchResultDto(
    int ItemId,
    string CustomId,
    int InventoryId,
    string InventoryTitle,
    List<string> MatchedFields);
