namespace InventoryApp.Application.Configurations;

public sealed class ImageKitSettings
{
    public const string SectionName = nameof(ImageKitSettings);

    public string PublicKey { get; set; }
    public string PrivateKey { get; set; }
    public string UrlEndpoint { get; set; }
}
