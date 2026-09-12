namespace Shared.Models;

public class FileAttachment
{
    public int Id { get; set; }
    public int MessageId { get; set; }
    public Message Message { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public string FileUrl { get; set; } = null!;
}
