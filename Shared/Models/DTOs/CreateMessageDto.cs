namespace Shared.Models.DTOs
{
    public class CreateMessageDto
    {
        public int? ReceiverId { get; set; }
        public int? GroupId    { get; set; }
        public string Content  { get; set; }
    }
}
