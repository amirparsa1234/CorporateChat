namespace Shared.Models;
public class Message
{
    public int      Id          { get; set; }
    public int      SenderId    { get; set; }
    public User     Sender      { get; set; } = null!;
    public int?     ReceiverId  { get; set; }
    public User?    Receiver    { get; set; }
    public int?     GroupId     { get; set; }
    public Group?   Group       { get; set; }
    public string   Content     { get; set; } = null!;
    public DateTime SentAt      { get; set; }
    public bool     IsRead      { get; set; }
    public ICollection<FileAttachment> Attachments { get; set; } = new List<FileAttachment>();
}
