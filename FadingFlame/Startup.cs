using System;
using System.IdentityModel.Tokens.Jwt;
using FadingFlame.Discord;
using FadingFlame.Leagues;
using FadingFlame.Players;
using FadingFlame.UserAccounts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace FadingFlame
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "Cookies";
                    options.DefaultChallengeScheme = "oidc";
                })
                .AddCookie("Cookies", options =>
                {
                    options.Events.OnSigningOut = async e =>
                    {
                        await e.HttpContext.RevokeUserRefreshTokenAsync();
                    };
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Authority = $"https://{Environment.GetEnvironmentVariable("IDENTITY_BASE_URI")}";

                    options.ClientId = "fading-flame";
                    var secret = Environment.GetEnvironmentVariable("FADING_FLAME_SECRET");
                    options.ClientSecret = secret;
                    options.ResponseType = "code";
                    options.SignedOutRedirectUri = $"https://{Environment.GetEnvironmentVariable("SIGNOUT_URI")}/signout-callback-oidc";

                    options.Scope.Add("profile");
                    options.Scope.Add("email");
                    options.Scope.Add("offline_access");
                    options.GetClaimsFromUserInfoEndpoint = true;

                    options.SaveTokens = true;
                });

            services.AddAccessTokenManagement()
                .ConfigureBackchannelHttpClient();
           
            services.AddSingleton(_ =>
            {
                var mongoConnectionString = Environment.GetEnvironmentVariable("MONGO_DB_CONNECTION_STRING");
                return new MongoClient(mongoConnectionString);
            });
            
            services.AddSingleton<IDiscordBot>(_ =>
            {
                var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
                var discordBot = new DiscordBot(token);
                return discordBot;
            });

            services.AddTransient<ILeagueRepository, LeagueRepository>();
            services.AddTransient<IPlayerRepository, PlayerRepository>();
            services.AddTransient<ILeagueCommandHandler, LeagueCommandHandler>();
            services.AddScoped<UserState>();
            services.AddHttpContextAccessor();

            // var buildServiceProvider = services.BuildServiceProvider();
            // var playerRepository = buildServiceProvider.GetService<IPlayerRepository>();
            // for (int i = 0; i < 107; i++)
            // {
            //     var player = Player.Create($"testuser{i}", $"testuser{i}@lel.de");
            //     playerRepository.Insert(player).Wait();
            // }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();

            app.UseRouting();
            
            // hack for identity server and caddy
            app.Use((context, next) =>
            {
                context.Request.Scheme = "https";
                return next();
            });
            
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}