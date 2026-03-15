using InventoryApp.Application.Interfaces;
using InventoryApp.Domain.Entities;
using InventoryApp.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Text;

namespace InventoryApp.Infrastructure.Services;

internal sealed class CustomIdGenerator(ILogger<CustomIdGenerator> logger) : ICustomIdGenerator
{
    public string Generate(List<InventoryIdFormatPart> parts, int nextSequence)
    {
        var sb = new StringBuilder();

        foreach (var part in parts.OrderBy(x => x.Order))
        {
            sb.Append(part.Type switch
            {
                IdFormatPartType.FixedText => part.Config ?? "",
                IdFormatPartType.Sequence => nextSequence.ToString(part.Config ?? "000"),
                IdFormatPartType.Random6Digit => Random.Shared.Next(100000, 999999).ToString(),
                IdFormatPartType.Random9Digit => Random.Shared.Next(100000000, 999999999).ToString(),
                IdFormatPartType.Guid => Guid.NewGuid().ToString("N")[..8].ToUpper(),
                IdFormatPartType.DateTime => DateTime.UtcNow.ToString(part.Config ?? "yyyyMMdd"),
                _ => ""
            });
        }

        var generatedId = sb.ToString();
        logger.LogInformation("Generated custom ID: {GeneratedId} for sequence {NextSequence}.", generatedId, nextSequence);
        return generatedId;
    }
}