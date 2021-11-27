using System;
using System.IdentityModel.Tokens.Jwt;
using FadingFlame.Admin;
using FadingFlame.Discord;
using FadingFlame.GlobalLadder;
using FadingFlame.Leagues;
using FadingFlame.Lists;
using FadingFlame.Matchups;
using FadingFlame.Players;
using FadingFlame.Playoffs;
using FadingFlame.ReadModelBase;
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
            services.AddControllers()
                .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.Converters.Add(new ObjectIdConverter());
                    }
                );
            
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

            services.AddHttpClient<IMmrRepository, MmrRepository>(c =>
            {
                var mmrUri = Environment.GetEnvironmentVariable("MMR_SERVICE_URI") ?? "https://mmr-service.w3champions.com";
                c.BaseAddress = new Uri(mmrUri);
            }); 
            
            services.AddHttpClient<IGeoLocationService, GeoLocationService>(c =>
            {
                c.BaseAddress = new Uri("http://www.geoplugin.net/json.gp");
            });
            
            services.AddHttpClient<IListValidationService, ListValidationService>(c =>
            {
                c.BaseAddress = new Uri("http://newrecruit.eu");
            });

            services.AddAccessTokenManagement()
                .ConfigureBackchannelHttpClient();
           
            services.AddSingleton(_ =>
            {
                var mongoConnectionString = Environment.GetEnvironmentVariable("MONGO_DB_CONNECTION_STRING");
                return new MongoClient(mongoConnectionString);
            });
            
            services.AddSingleton<IDiscordBot>(option =>
            {
                var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
                var discordBot = new DiscordBot(token, option.GetService<IPlayerRepository>());
                return discordBot;
            });

            services.AddTransient<IChallengeService, ChallengeService>();
            services.AddTransient<IVersionRepository, VersionRepository>();
            services.AddTransient<IRankingReadmodelRepository, RankingReadmodelRepository>();
            services.AddTransient<ILeagueRepository, LeagueRepository>();
            services.AddTransient<IPlayerRepository, PlayerRepository>();
            services.AddTransient<IPlayoffCommandHandler, PlayoffCommandHandler>();
            services.AddTransient<IPlayoffRepository, PlayoffRepository>();
            services.AddTransient<ISeasonRepository, SeasonRepository>();
            services.AddTransient<IMatchupRepository, MatchupRepository>();
            services.AddTransient<ILeagueCreationService, LeagueCreationService>();
            services.AddTransient<IListAcceptAndRejectService, ListAcceptAndRejectService>();
            services.AddTransient<IListRepository, ListRepository>();
            services.AddScoped<LoggedInUserState>();
            services.AddScoped<SeasonState>();
            services.AddHttpContextAccessor();
            services.AddReadModelService<PlayerRankingModelReadHandler>();
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
                endpoints.MapControllers();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
    
    public static class ReadModelExtensions
    {
        public static IServiceCollection AddReadModelService<TService>(this IServiceCollection services) where TService : class, IAsyncUpdatable
        {
            services.AddTransient<TService>();
            services.AddSingleton<IHostedService, AsyncService<TService>>();
            return services;
        }
    }
}