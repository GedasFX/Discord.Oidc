using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Discord;
using Discord.WebSocket;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Discord.Oidc.Controllers
{
    public class AccountController : Controller
    {
        private readonly PkiService _pkiService;
        private readonly DiscordApiHttpClient _api;
        private readonly DiscordSocketClient _bot;

        public AccountController(PkiService pkiService, DiscordApiHttpClient api, DiscordSocketClient bot)
        {
            _pkiService = pkiService;
            _api = api;
            _bot = bot;
        }

        [HttpGet("login")]
        [Authorize(AuthenticationSchemes = DiscordAuthenticationDefaults.AuthenticationScheme)]
        public Task LoginAsync()
        {
            // Signed in because of Authorize attribute
            return Task.CompletedTask;
        }

        [HttpPost("token")]
        [Authorize]
        public async Task<ActionResult<string>> TokenAsync()
        {
            var nameId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userGuilds = await _api.GetUserGuildsAsync(User.FindFirstValue("discord"));
            var mutualGuilds = userGuilds.Where(userGuild =>
                _bot.GetGuild(ulong.Parse(userGuild.Id, CultureInfo.InvariantCulture)) != null).ToList();

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new("gid", string.Join(',', mutualGuilds)),
                    new("sub", nameId),
                    new("iss", $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}"),
                    new("aud", "api"),
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = _pkiService.SigningCredentials
            });

            return tokenHandler.WriteToken(token);
        }

        public async Task LogoutAsync()
        {
            await HttpContext.SignOutAsync();
        }
    }
}
