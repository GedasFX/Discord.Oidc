using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Discord.Oidc
{
    public class DiscordApiHttpClient
    {
        private const string UserPath = "/api/v9/users/@me";
        private const string UserGuildsPath = "api/v9/users/@me/guilds";

        private readonly HttpClient _httpClient;

        public DiscordApiHttpClient(HttpClient httpClient)
        {
            httpClient.BaseAddress = new Uri("https://discordapp.com");

            _httpClient = httpClient;
        }

        public Task<DiscordApiUser> GetUserAsync(string accessToken)
            => GetResourceAsync<DiscordApiUser>(accessToken, UserPath);

        public Task<IList<DiscordApiGuild>> GetUserGuildsAsync(string discordAccessToken)
            => GetResourceAsync<IList<DiscordApiGuild>>(discordAccessToken, UserGuildsPath);

        private async Task<T> GetResourceAsync<T>(string discordAccessToken, string path) where T : class
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", discordAccessToken);

            var response = await _httpClient.SendAsync(request);

            var o = await JsonSerializer.DeserializeAsync<T>(await response.Content.ReadAsStreamAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return o!;
        }
    }

    public class DiscordApiGuild
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Permissions { get; set; }
        public bool Owner { get; set; }
        public string Icon { get; set; }

        public DiscordApiGuild(string id, string name, string permissions, bool owner, string icon)
        {
            Id = id;
            Name = name;
            Permissions = permissions;
            Owner = owner;
            Icon = icon;
        }
    }

    public class DiscordApiUser
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Discriminator { get; set; }

        public string? Avatar { get; set; }
        public bool? Bot { get; set; }

        [JsonPropertyName("mfa_enabled")]
        public bool? MfaEnabled { get; set; }

        public string? Locale { get; set; }

        [JsonPropertyName("verified")]
        public bool? EmailVerified { get; set; }

        public string? Email { get; set; }

        public int? Flags { get; set; }

        [JsonPropertyName("premium_type")]
        public int? PremiumType { get; set; }

        public IEnumerable<DiscordApiGuild>? Guilds { get; set; }

        public DiscordApiUser(string id, string username, string discriminator, string? avatar = null, bool? bot = null,
            bool? mfaEnabled = null, string? locale = null, bool? emailVerified = null, string? email = null,
            int? flags = null, int? premiumType = null)
        {
            Id = id;
            Username = username;
            Discriminator = discriminator;
            Avatar = avatar;
            Bot = bot;
            MfaEnabled = mfaEnabled;
            Locale = locale;
            EmailVerified = emailVerified;
            Email = email;
            Flags = flags;
            PremiumType = premiumType;
        }
    }
}
