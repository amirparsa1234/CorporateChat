namespace Shared.Models;
public class User
{
    public int    Id           { get; set; }
    public string Username     { get; set; } = null!;
    public string Email        { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public bool   IsOnline     { get; set; }
    public ICollection<Message> SentMessages   { get; set; } = new List<Message>();
    public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
}