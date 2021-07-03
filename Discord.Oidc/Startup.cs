using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(o =>
                {
                    o.ExpireTimeSpan = TimeSpan.FromDays(7);
                    o.LoginPath = PathString.Empty;
                })
                .AddDiscord(options =>
                    {
                        options.ClientId = Configuration["DiscordAPI:ClientId"];
                        options.ClientSecret = Configuration["DiscordAPI:ClientSecret"];
                        options.SaveTokens = true;

                        options.Scope.Add("guilds");

                        options.Events.OnCreatingTicket = c =>
                        {
                            c.Identity.AddClaim(new Claim("discord", c.AccessToken));
                            return Task.CompletedTask;
                        };
                    }
                );
            services.AddAuthorization();

            services.AddHttpClient<DiscordApiHttpClient>();
            services.AddSingleton<PkiService>();
            services.AddSingleton(f =>
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
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
