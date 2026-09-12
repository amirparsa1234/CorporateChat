using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Shared.Models;
using Shared.Models.DTOs;

namespace Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly ChatDbContext _db;
        public MessagesController(ChatDbContext db) => _db = db;

        [HttpGet("conversation")]
        public async Task<IActionResult> GetConversation([FromQuery] int userId, [FromQuery] int otherUserId)
        {
            var msgs = await _db.Messages
                .Include(m => m.Attachments)
                .Where(m =>
                    (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                    (m.SenderId == otherUserId && m.ReceiverId == userId))
                .OrderBy(m => m.SentAt)
                .ToListAsync();
            return Ok(msgs);
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> Post([FromBody] CreateMessageDto dto)
        {
            var sid = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var msg = new Message
            {
                SenderId   = sid,
                ReceiverId = dto.ReceiverId,
                GroupId    = dto.GroupId,
                Content    = dto.Content,
                SentAt     = DateTime.UtcNow
            };
            _db.Messages.Add(msg);
            await _db.SaveChangesAsync();
            var outDto = new MessageDto
            {
                Id         = msg.Id,
                SenderId   = msg.SenderId,
                ReceiverId = msg.ReceiverId,
                GroupId    = msg.GroupId,
                Content    = msg.Content,
                SentAt     = msg.SentAt
            };
            return CreatedAtAction(nameof(GetConversation), new { userId = sid, otherUserId = dto.ReceiverId }, outDto);
        }
    }
}
