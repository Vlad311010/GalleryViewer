using System.Text.Json.Serialization;

namespace App.Enum
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DisplayItemType
    {
        Asset,
        Group
    }
}
