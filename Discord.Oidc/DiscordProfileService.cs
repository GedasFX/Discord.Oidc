using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Discord.WebSocket;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.DataProtection;

namespace Discord.Oidc
{
    public class DiscordProfileService : IProfileService
    {
        private readonly DiscordApiHttpClient _api;
        private readonly DiscordSocketClient _bot;
        private readonly IDataProtector _protector;

        public DiscordProfileService(IDataProtectionProvider dpProvider, DiscordApiHttpClient api,
            DiscordSocketClient bot)
        {
            _api = api;
            _bot = bot;
            _protector = dpProvider.CreateProtector(nameof(DiscordProfileService));
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            switch (context.Caller)
            {
                // Uses idsrv access token.
                case IdentityServerConstants.ProfileDataCallers.UserInfoEndpoint:
                {
                    var accessToken = _protector.Unprotect(context.Subject.FindFirstValue("discord"));

                    var user = await _api.GetUserAsync(accessToken);
                    var userGuilds = await _api.GetUserGuildsAsync(accessToken);

                    var mutualGuilds = userGuilds.Where(userGuild =>
                        _bot.GetGuild(ulong.Parse(userGuild.Id, CultureInfo.InvariantCulture)) != null);
                    user.Guilds = mutualGuilds;

                    var serializerOptions = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        IgnoreNullValues = true
                    };

                    context.IssuedClaims
                        .Add(new Claim("user", JsonSerializer.Serialize(user, serializerOptions), "json"));

                    return;
                }

                // Uses idsrv cookie.
                case IdentityServerConstants.ProfileDataCallers.ClaimsProviderAccessToken:
                {
                    var accessToken = context.Subject.FindFirstValue("discord");
                    context.IssuedClaims.Add(new Claim("discord", _protector.Protect(accessToken)));

                    return;
                }
            }
        }

        public Task IsActiveAsync(IsActiveContext context) => Task.CompletedTask;
    }
}
