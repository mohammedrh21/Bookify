using System.Text.Json.Serialization;

namespace Bookify.Client.Models.Profile
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum GenderType
    {
        Female = 0,
        Male = 1,
        PreferNotToSay = 2
    }
}
