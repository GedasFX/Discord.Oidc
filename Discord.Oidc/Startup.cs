using System.Security.Claims;
using System.Threading.Tasks;
using Discord.WebSocket;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Discord.Oidc
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ForwardedHeadersOptions>(o =>
            {
                o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                o.KnownNetworks.Clear();
                o.KnownProxies.Clear();
            });

            services.AddIdentityServer(o => { o.UserInteraction.LoginUrl = "/login"; })
                .AddInMemoryIdentityResources(OidcResources.IdentityResources)
                .AddInMemoryApiResources(OidcResources.ApiResources)
                .AddInMemoryApiScopes(OidcResources.ApiScopes)
                .AddInMemoryClients(OidcResources.Clients(
                    Configuration["OAuth:RedirectUris"],
                    Configuration["OAuth:PostLogoutRedirectUris"],
                    Configuration["OAuth:AllowedCorsOrigins"]))
                .AddProfileService<DiscordProfileService>()
                .AddDeveloperSigningCredential(filename: "token.rsa");

            services.AddAuthentication()
                .AddDiscord(options =>
                    {
                        options.ClientId = Configuration["DiscordAPI:ClientId"];
                        options.ClientSecret = Configuration["DiscordAPI:ClientSecret"];
                        options.SaveTokens = true;

                        options.Scope.Add("guilds");

                        options.ClaimActions.MapJsonKey(JwtClaimTypes.Subject, "id");

                        options.Events.OnCreatingTicket = c =>
                        {
                            c.Identity.AddClaim(new Claim("discord", c.AccessToken));
                            return Task.CompletedTask;
                        };
                    }
                );
            services.AddAuthorization();

            services.AddHttpClient<DiscordApiHttpClient>();
            services.AddSingleton(_ =>
            {
                var client = new DiscordSocketClient();

                Task.Run(() =>
                {
                    client.LoginAsync(TokenType.Bot, Configuration["DiscordAPI:BotToken"]);
                    client.StartAsync();
                });

                return client;
            });

            services.AddMvcCore()
                .AddDataAnnotations()
                .AddJsonOptions(o => { o.JsonSerializerOptions.IgnoreNullValues = true; });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders();

            app.UseRouting();

            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
