using System.Text.Json.Serialization;

namespace TelegramForwardly.WebApi.Models.Dtos
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum GroupingMode
    {
        ByKeyword,
        ByChat,
        General
    }
}
