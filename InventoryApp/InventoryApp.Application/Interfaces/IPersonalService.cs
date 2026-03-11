using InventoryApp.Application.DTOs.Personal;

namespace InventoryApp.Application.Interfaces;

public interface IPersonalService
{
    Task<PersonalStatsDto> GetMyStatsAsync();
}
