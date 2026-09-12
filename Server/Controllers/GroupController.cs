using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Shared.Models;
using Shared.Models.DTOs;

namespace Server.Controllers
{
    public class AddMemberDto
    {
        public int UserId { get; set; }
        public GroupRole Role { get; set; }
    }

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GroupsController : ControllerBase
    {
        private readonly ChatDbContext _db;
        public GroupsController(ChatDbContext db) => _db = db;

        private int GetCurrentUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private async Task<GroupMember?> GetMembership(int gid, int uid) =>
            await _db.GroupMembers.FirstOrDefaultAsync(x => x.GroupId == gid && x.UserId == uid);

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Group>>> GetMy()
        {
            var uid = GetCurrentUserId();
            var groups = await _db.Groups
                .Include(g => g.Members)
                .ThenInclude(m => m.User)
                .Where(g => g.Members.Any(m => m.UserId == uid))
                .ToListAsync();
            return Ok(groups);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id)
        {
            var uid = GetCurrentUserId();
            var group = await _db.Groups
                .Include(g => g.Members).ThenInclude(m => m.User)
                .AsSplitQuery()
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null || !group.Members.Any(m => m.UserId == uid))
                return NotFound();

            var result = new
            {
                group.Id,
                group.Name,
                group.OwnerId,
                Members = group.Members.Select(m => new
                {
                    m.GroupId,
                    m.UserId,
                    m.Role,
                    User = new { m.User.Id, m.User.UserName }
                })
            };

            return Ok(result);
        }

        [HttpGet("{id}/messages")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessages(int id)
        {
            var uid = GetCurrentUserId();
            var isMember = await _db.GroupMembers.AnyAsync(m => m.GroupId == id && m.UserId == uid);
            if (!isMember) return NotFound();
            
            var messages = await _db.Messages
                .Where(m => m.GroupId == id)
                .OrderBy(m => m.SentAt)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    GroupId = m.GroupId,
                    Content = m.Content,
                    SentAt = m.SentAt,
                    SenderUsername = _db.Users
                        .Where(u => u.Id == m.SenderId)
                        .Select(u => u.UserName)
                        .FirstOrDefault()
                })
                .ToListAsync();
            return Ok(messages);
        }

        [HttpPost]
        public async Task<ActionResult<Group>> Create(CreateGroupDto dto)
        {
            var uid = GetCurrentUserId();
            var group = new Group { Name = dto.Name, OwnerId = uid };
            _db.Groups.Add(group);
            await _db.SaveChangesAsync();
            
            _db.GroupMembers.Add(new GroupMember { GroupId = group.Id, UserId = uid, Role = GroupRole.Owner });
            await _db.SaveChangesAsync();
            
            return Ok(new { group.Id, group.Name });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var uid = GetCurrentUserId();
            var caller = await GetMembership(id, uid);
            if (caller == null || caller.Role != GroupRole.Owner) return Forbid();
            
            var group = await _db.Groups.FindAsync(id);
            if (group == null) return NotFound();
            
            _db.Groups.Remove(group);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/members")]
        public async Task<IActionResult> AddMember(int id, [FromBody] AddMemberDto dto)
        {
            var uid = GetCurrentUserId();
            var caller = await GetMembership(id, uid);
            
            if (caller == null || (caller.Role != GroupRole.Owner && caller.Role != GroupRole.Admin)) return Forbid();
            if (dto.Role == GroupRole.Owner) return BadRequest();
            if (caller.Role == GroupRole.Admin && dto.Role != GroupRole.Member) return Forbid();
            
            var exists = await GetMembership(id, dto.UserId);
            if (exists != null) return BadRequest();
            
            var member = new GroupMember { GroupId = id, UserId = dto.UserId, Role = dto.Role };
            _db.GroupMembers.Add(member);
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}/members/{userId}")]
        public async Task<IActionResult> RemoveMember(int id, int userId)
        {
            var uid = GetCurrentUserId();
            var caller = await GetMembership(id, uid);
            
            if (caller == null || (caller.Role != GroupRole.Owner && caller.Role != GroupRole.Admin)) return Forbid();
            
            var target = await GetMembership(id, userId);
            if (target == null) return NotFound();
            
            if (target.Role == GroupRole.Owner) return Forbid();
            if (caller.Role == GroupRole.Admin && target.Role != GroupRole.Member) return Forbid();
            
            _db.GroupMembers.Remove(target);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/members/{userId}/role")]
        public async Task<IActionResult> ChangeRole(int id, int userId, [FromBody] GroupRole role)
        {
            var uid = GetCurrentUserId();
            var caller = await GetMembership(id, uid);
            var target = await GetMembership(id, userId);
            
            if (caller == null || caller.Role != GroupRole.Owner) return Forbid();
            if (target == null) return NotFound();
            if (target.Role == GroupRole.Owner) return Forbid();
            if (role == GroupRole.Owner) return Forbid();
            
            target.Role = role;
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}/leave")]
        public async Task<IActionResult> Leave(int id)
        {
            var uid = GetCurrentUserId();
            var caller = await GetMembership(id, uid);
            if (caller == null) return NotFound();

            if (caller.Role == GroupRole.Owner)
            {
                var group = await _db.Groups
                    .Include(g => g.Members)
                    .Include(g => g.Messages)
                    .FirstOrDefaultAsync(g => g.Id == id);
                    
                if (group != null)
                {
                    _db.Messages.RemoveRange(group.Messages);
                    _db.GroupMembers.RemoveRange(group.Members);
                    _db.Groups.Remove(group);
                    await _db.SaveChangesAsync();
                }
                return NoContent();
            }

            _db.GroupMembers.Remove(caller);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
