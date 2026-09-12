using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Shared.Models;
using Shared.Models.DTOs;

namespace Server.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ChatDbContext _db;
        
        public ChatHub(ChatDbContext db) => _db = db;

        public override async Task OnConnectedAsync()
        {
            var u = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(u))
            {
                var userId = int.Parse(u);
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

                var myGroups = await _db.GroupMembers
                    .Where(gm => gm.UserId == userId)
                    .Select(gm => gm.GroupId)
                    .ToListAsync();

                foreach (var gid in myGroups)
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"group-{gid}");
            }
            await base.OnConnectedAsync();
        }

        public async Task SendMessage(SendMessageDto dto)
        {
            var u = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(u)) return;

            var senderId = int.Parse(u);
            var senderUser = await _db.Users.FindAsync(senderId);

            var msg = new Message
            {
                SenderId = senderId,
                ReceiverId = dto.ReceiverId,
                GroupId = dto.GroupId,
                Content = dto.Content ?? string.Empty,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            if (!string.IsNullOrEmpty(dto.FileUrl))
            {
                msg.Attachments.Add(new FileAttachment
                {
                    FileUrl = dto.FileUrl,
                    FileName = dto.FileName!,
                    ContentType = dto.ContentType!
                });
            }

            _db.Messages.Add(msg);
            await _db.SaveChangesAsync();

            var outMsg = new
            {
                Id = msg.Id,
                SenderId = senderId,
                SenderUsername = senderUser?.UserName,
                ReceiverId = dto.ReceiverId,
                GroupId = dto.GroupId,
                Content = dto.Content,
                SentAt = msg.SentAt,
                Attachments = msg.Attachments.Select(a => new { a.FileUrl, a.FileName, a.ContentType })
            };

            if (dto.ReceiverId.HasValue)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", outMsg);
                await Clients.Group($"user-{dto.ReceiverId.Value}").SendAsync("ReceiveMessage", outMsg);
            }
            else if (dto.GroupId.HasValue)
            {
                await Clients.Group($"group-{dto.GroupId.Value}").SendAsync("ReceiveGroupMessage", outMsg);
            }
        }

        public async Task JoinGroup(int groupId)
        {
            var u = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(u)) throw new HubException("Unauthorized");

            var userId = int.Parse(u);
            var gm = await _db.GroupMembers.FindAsync(groupId, userId);

            if (gm == null) throw new HubException("Not a member.");
            await Groups.AddToGroupAsync(Context.ConnectionId, $"group-{groupId}");
        }

        public Task LeaveGroup(int groupId) =>
            Groups.RemoveFromGroupAsync(Context.ConnectionId, $"group-{groupId}");
    }
}
