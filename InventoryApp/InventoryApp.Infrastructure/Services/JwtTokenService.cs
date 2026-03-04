using InventoryApp.Application.Configurations;
using InventoryApp.Application.Interfaces;
using InventoryApp.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InventoryApp.Infrastructure.Services;

internal class JwtTokenService(
    IOptions<JwtSettings> options,
    ILogger<JwtTokenService> logger) : IJwtTokenService
{
    private readonly JwtSettings jwtSettings = options.Value;
    public string GenerateToken(User user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier,user.Id),
            new(ClaimTypes.Email,user.Email!),
            new(ClaimTypes.Name,user.DisplayName)
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.Key));

        var expires = DateTime.UtcNow.AddDays(7);

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        logger.LogInformation(
            "JWT generated. UserId={UserId} Email={Email} Roles={Roles} ExpiresAtUtc={Expires}",
            user.Id, user.Email, roles, expires);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
