using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AspNet.Security.OAuth.Discord;
using Discord.WebSocket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Discord.Oidc.Controllers
{
    public class AccountController : Controller
    {
        private readonly PkiService _pkiService;
        private readonly DiscordSocketClient _discord;

        public AccountController(PkiService pkiService, DiscordSocketClient discord)
        {
            _pkiService = pkiService;
            _discord = discord;
        }

        [HttpGet("login")]
        [Authorize(AuthenticationSchemes = DiscordAuthenticationDefaults.AuthenticationScheme)]
        public void LoginAsync()
        {
        }

        [HttpPost("login/{guildId}")]
        [Authorize]
        public ActionResult<string> TokenAsync(ulong guildId)
        {
            var nameId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (nameId == null || !ulong.TryParse(nameId, out var userId))
                return Unauthorized();

            // Check if user exists in a given guild
            if (_discord.GetGuild(guildId)?.GetUser(userId) == null)
                return Forbid();

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new("gid", guildId.ToString()),
                    new("sub", nameId),
                    new("iss", $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}"),
                    new("aud", "api"),
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = _pkiService.SigningCredentials
            });

            return tokenHandler.WriteToken(token);
        }
    }
}
