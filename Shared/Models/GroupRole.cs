using System.Text.Json.Serialization;

namespace Shared.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum GroupRole
    {
        Owner,
        Admin,
        Member
    }
}
