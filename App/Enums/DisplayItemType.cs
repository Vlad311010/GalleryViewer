using System.Text.Json.Serialization;

namespace App.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DisplayItemType
    {
        Asset,
        Group
    }
}
