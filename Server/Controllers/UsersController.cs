using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Shared.Models.DTOs;

namespace Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ChatDbContext _db;
        public UsersController(ChatDbContext db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
        {
            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(currentUserIdStr, out int currentUserId);

            var users = await _db.Users
                .Where(u => u.Id != currentUserId)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    UserName = u.UserName!
                })
                .ToListAsync();
            
            return Ok(users);
        }
    }
}
