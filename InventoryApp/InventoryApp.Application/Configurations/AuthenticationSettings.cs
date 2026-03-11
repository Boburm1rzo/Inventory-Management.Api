namespace InventoryApp.Application.Configurations;

public sealed class AuthenticationSettings
{
    public const string SectionName = nameof(AuthenticationSettings);
    public string GoogleClientId { get; set; }
    public string GoogleClientSecret { get; set; }
    public string FacebookAppId { get; set; }
    public string FacebookAppSecret { get; set; }
}
