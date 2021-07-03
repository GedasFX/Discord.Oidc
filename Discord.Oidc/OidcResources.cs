using IdentityServer4.Models;

namespace Discord.Oidc
{
    public static class OidcResources
    {
        public static readonly IdentityResource[] IdentityResources =
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile()
        };

        public static readonly ApiScope[] ApiScopes =
        {
            new("api")
        };

        public static readonly ApiResource[] ApiResources =
        {
            new("api")
            {
                Scopes = { "api" }
            }
        };

        public static Client[] Clients(string redirectUris, string postLogoutRedirectUris, string allowedCorsOrigins) =>
            new[]
            {
                new Client
                {
                    ClientId = "js",
                    ClientName = "Single Page Application",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    RequireClientSecret = false,
                    RequireConsent = false,

                    RedirectUris = redirectUris.Split(';'),
                    PostLogoutRedirectUris = postLogoutRedirectUris.Split(';'),
                    AllowedCorsOrigins = allowedCorsOrigins.Split(';'),

                    AllowOfflineAccess = true,
                    RefreshTokenUsage = TokenUsage.OneTimeOnly,

                    AllowAccessTokensViaBrowser = true,

                    AllowedScopes = { "api", "openid" }
                }
            };
    }
}
