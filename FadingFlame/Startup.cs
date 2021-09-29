using System;
using System.IdentityModel.Tokens.Jwt;
using FadingFlame.Admin;
using FadingFlame.Discord;
using FadingFlame.Leagues;
using FadingFlame.Matchups;
using FadingFlame.Players;
using FadingFlame.Playoffs;
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
                    options.Authority = Environment.GetEnvironmentVariable("IDENTITY_BASE_URI") != null ? $"https://{Environment.GetEnvironmentVariable("IDENTITY_BASE_URI")}" : "https://test.identity.fading-flame.com/";
                    options.ClientSecret = Environment.GetEnvironmentVariable("FADING_FLAME_SECRET") ?? "NnsTfzUSswFVARczALPmncmKvT53j7zN";
                    options.SignedOutRedirectUri = Environment.GetEnvironmentVariable("SIGNOUT_URI") != null ? $"https://{Environment.GetEnvironmentVariable("SIGNOUT_URI")}/signout-callback-oidc" : "https://localhost:5000/signout-callback-oidc";

                    options.ClientId = "fading-flame";
                    options.ResponseType = "code";
                   
                    options.Scope.Add("profile");
                    options.Scope.Add("email");
                    options.Scope.Add("offline_access");
                    options.GetClaimsFromUserInfoEndpoint = true;

                    options.SaveTokens = true;
                });

            services.AddHttpClient<MmrRepository>(c =>
            {
                var mmrUri = Environment.GetEnvironmentVariable("MMR_SERVICE_URI") ?? "https://mmr-service.w3champions.com";
                c.BaseAddress = new Uri(mmrUri);
            }); 
            
            services.AddHttpClient<IGeoLocationService, GeoLocationService>(c =>
            {
                c.BaseAddress = new Uri("http://www.geoplugin.net/json.gp");
            });

            services.AddAccessTokenManagement()
                .ConfigureBackchannelHttpClient();
           
            services.AddSingleton(_ =>
            {
                var mongoConnectionString = Environment.GetEnvironmentVariable("MONGO_DB_CONNECTION_STRING") ?? "mongodb://admin:vgwG9FRzS77tGGP4@65.21.139.246:1001";
                return new MongoClient(mongoConnectionString);
            });
            
            services.AddSingleton<IDiscordBot>(option =>
            {
                var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
                var discordBot = new DiscordBot(token, option.GetService<IPlayerRepository>());
                return discordBot;
            });

            services.AddTransient<ILeagueRepository, LeagueRepository>();
            services.AddTransient<IPlayerRepository, PlayerRepository>();
            services.AddTransient<IPlayoffCommandHandler, PlayoffCommandHandler>();
            services.AddTransient<IPlayoffRepository, PlayoffRepository>();
            services.AddTransient<ISeasonRepository, SeasonRepository>();
            services.AddTransient<IMatchupRepository, MatchupRepository>();
            services.AddScoped<UserState>();
            services.AddScoped<SeasonState>();
            services.AddHttpContextAccessor();
            
            // var buildServiceProvider = services.BuildServiceProvider();
            // var playerRepository = buildServiceProvider.GetService<IPlayerRepository>();
            // for (int i = 0; i < 107; i++)
            // {
            //     var player = Player.Create($"testuser{i}@lel.de", $"testuser{i}@lel.de");
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