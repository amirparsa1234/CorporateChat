using System.Text.Json.Serialization;

namespace Shared.Models
{
    public class GroupMember
    {
        public int GroupId { get; set; }
        public int UserId { get; set; }
        public GroupRole? Role { get; set; } = GroupRole.Member;

        [JsonIgnore]
        public Group Group { get; set; } = null!;

        public User User { get; set; } = null!;
    }
}
