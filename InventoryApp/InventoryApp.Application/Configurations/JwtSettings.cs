namespace InventoryApp.Application.Configurations;

public sealed class JwtSettings
{
    public const string SectionName = nameof(JwtSettings);

    public string Key { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
}
