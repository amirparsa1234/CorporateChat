namespace Shared.Models.DTOs;

public class SendMessageDto
{
    public int? ReceiverId { get; set; }
    public int? GroupId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
}
