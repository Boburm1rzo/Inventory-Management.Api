using InventoryApp.Application.DTOs.Search;

namespace InventoryApp.Application.Interfaces;

public interface ISearchService
{
    Task<SearchResultDto> SearchAsync(string query);
}
