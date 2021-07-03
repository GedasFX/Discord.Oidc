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
        private readonly DiscordApiHttpClient _api;
        private readonly DiscordSocketClient _bot;

        public UserController(DiscordApiHttpClient api, DiscordSocketClient bot)
        {
            _api = api;
            _bot = bot;
        }

        [HttpGet]
        public async Task<ActionResult<DiscordApiUser>> GetMeAsync()
        {
            return await _api.GetUserAsync(User.FindFirstValue("discord"));
        }

        [HttpGet("guilds")]
        public async Task<ActionResult<IEnumerable<DiscordApiGuild>>> GetMyGuildsAsync()
        {
            var userGuilds = await _api.GetUserGuildsAsync(User.FindFirstValue("discord"));
            return userGuilds.Where(userGuild =>
                _bot.GetGuild(ulong.Parse(userGuild.Id, CultureInfo.InvariantCulture)) != null).ToList();
        }
    }
}
