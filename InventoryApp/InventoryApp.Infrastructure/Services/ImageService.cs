using Imagekit.Sdk;
using InventoryApp.Application.Configurations;
using InventoryApp.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace InventoryApp.Infrastructure.Services;

internal sealed class ImageService(IOptions<ImageKitSettings> settings) : IImageService
{
    private readonly ImagekitClient _client = new(
        settings.Value.PublicKey,
        settings.Value.PrivateKey,
        settings.Value.UrlEndpoint);

    public async Task<string> UploadImageAsync(Stream stream, string fileName)
    {
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        var bytes = ms.ToArray();

        var uploadRequest = new FileCreateRequest
        {
            file = Convert.ToBase64String(bytes),
            fileName = $"{Guid.NewGuid()}_{fileName}",
            folder = "/inventory-app"
        };

        var result = await _client.UploadAsync(uploadRequest);

        if (result.HttpStatusCode != 200)
            throw new Exception($"ImageKit upload failed: {result.metadata}");

        return result.url;
    }

    public async Task DeleteImageAsync(string fileId)
    {
        await _client.DeleteFileAsync(fileId);
    }
}
