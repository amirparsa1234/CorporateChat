using System;

namespace Shared.Models.DTOs
{
    public class MessageDto
    {
        public int    Id             { get; set; }
        public int    SenderId       { get; set; }
        public string SenderUsername { get; set; }
        public int?   ReceiverId     { get; set; }
        public int?   GroupId        { get; set; }
        public string Content        { get; set; }
        public DateTime SentAt       { get; set; }
    }
}
