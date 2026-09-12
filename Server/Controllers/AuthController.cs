using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Server.Data;
using Shared.Models;
using Shared.Models.DTOs;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ChatDbContext _db;
        private readonly IPasswordHasher<User> _hasher;
        private readonly IConfiguration _cfg;

        public AuthController(
            ChatDbContext db,
            IPasswordHasher<User> hasher,
            IConfiguration cfg)
        {
            _db = db;
            _hasher = hasher;
            _cfg = cfg;
        }

        private string GenerateRefreshToken()
        {
            var rnd = new byte[32];
            RandomNumberGenerator.Fill(rnd);
            return Convert.ToBase64String(rnd);
        }

        private string CreateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(
                    JwtRegisteredClaimNames.Sub,
                    user.Id.ToString()),

                new Claim(
                    JwtRegisteredClaimNames.UniqueName,
                    user.UserName ?? string.Empty)
            };

            var keyValue = _cfg["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(keyValue))
            {
                throw new InvalidOperationException(
                    "JWT configuration key 'Jwt:Key' is missing.");
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(keyValue));

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var issuer = _cfg["Jwt:Issuer"];
            var audience = _cfg["Jwt:Audience"];
            var expireMinutesValue = _cfg["Jwt:ExpireMinutes"];

            if (!double.TryParse(
                    expireMinutesValue,
                    out var expireMinutes))
            {
                expireMinutes = 60;
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (await _db.Users.AnyAsync(
                    u => u.UserName == dto.UserName))
            {
                return BadRequest("Username taken");
            }

            var user = new User
            {
                UserName = dto.UserName,
                Email = dto.Email
            };

            user.PasswordHash = _hasher.HashPassword(
                user,
                dto.Password);

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var jwt = CreateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            _db.RefreshTokens.Add(
                new RefreshToken
                {
                    Token = refreshToken,
                    Expires = DateTime.UtcNow.AddDays(7),
                    UserId = user.Id
                });

            await _db.SaveChangesAsync();

            return Ok(
                new AuthResponseDto
                {
                    Token = jwt,
                    RefreshToken = refreshToken
                });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(
                u => u.UserName == dto.UserName);

            if (user == null ||
                _hasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    dto.Password) ==
                PasswordVerificationResult.Failed)
            {
                return Unauthorized();
            }

            var jwt = CreateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            _db.RefreshTokens.Add(
                new RefreshToken
                {
                    Token = refreshToken,
                    Expires = DateTime.UtcNow.AddDays(7),
                    UserId = user.Id
                });

            await _db.SaveChangesAsync();

            return Ok(
                new AuthResponseDto
                {
                    Token = jwt,
                    RefreshToken = refreshToken
                });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(
            RefreshRequestDto dto)
        {
            var stored = await _db.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(
                    rt => rt.Token == dto.RefreshToken);

            if (stored == null ||
                stored.Expires < DateTime.UtcNow)
            {
                return Unauthorized();
            }

            var user = stored.User;

            _db.RefreshTokens.Remove(stored);

            var newJwt = CreateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            _db.RefreshTokens.Add(
                new RefreshToken
                {
                    Token = newRefreshToken,
                    Expires = DateTime.UtcNow.AddDays(7),
                    UserId = user.Id
                });

            await _db.SaveChangesAsync();

            return Ok(
                new AuthResponseDto
                {
                    Token = newJwt,
                    RefreshToken = newRefreshToken
                });
        }
    }
}
