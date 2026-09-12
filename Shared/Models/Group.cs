using System.Text.Json.Serialization;

namespace Shared.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int OwnerId { get; set; }

        [JsonIgnore]
        public User Owner { get; set; } = null!;

        public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();

        [JsonIgnore]
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
