using System.Text.Json.Serialization;

namespace InventoryApp.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FieldType
{
    SingleLineText = 1,
    MultiLineText = 2,
    Numeric = 3,
    Link = 4,
    Boolean = 5,
}
