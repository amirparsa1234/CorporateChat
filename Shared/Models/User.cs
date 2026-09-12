using System.Text.Json.Serialization;

namespace Shared.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }

        [JsonIgnore]
        public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();

        [JsonIgnore]
        public virtual ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();

        [JsonIgnore]
        public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();
    }
}
