using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discord.Oidc.Controllers
{
    [ApiController, Authorize]
    [Route("users/@me")]
    public class UserController : ControllerBase
    {
        private readonly DiscordApiHttpClient _discord;
        private readonly DiscordSocketClient _bot;

        public UserController(DiscordApiHttpClient discord, DiscordSocketClient bot)
        {
            _discord = discord;
            _bot = bot;
        }

        [HttpGet]
        public async Task<ActionResult<DiscordApiUser>> GetMeAsync()
        {
            return await _discord.GetUserAsync(User.FindFirstValue("discord"));
        }

        [HttpGet("guilds")]
        public async Task<ActionResult<IEnumerable<DiscordApiGuild>>> GetMyGuildsAsync()
        {
            var userGuilds = await _discord.GetUserGuildsAsync(User.FindFirstValue("discord"));
            return userGuilds.Where(userGuild =>
                _bot.GetGuild(ulong.Parse(userGuild.Id, CultureInfo.InvariantCulture)) != null).ToList();
        }
    }
}
