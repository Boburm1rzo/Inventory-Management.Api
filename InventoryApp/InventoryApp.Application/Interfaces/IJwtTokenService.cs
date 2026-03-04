using InventoryApp.Domain.Entities;

namespace InventoryApp.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user, IList<string> roles);
}
