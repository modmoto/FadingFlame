using FadingFlame.Events;
using FadingFlame.Leagues;
using FadingFlame.Players;
using FadingFlame.UserAccounts;
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

            services.AddSingleton(_ =>
            {
                var mongoConnectionString = "mongodb://157.90.1.251:3513";
                return new MongoClient(mongoConnectionString);
            });

            services.AddTransient<ILeagueRepository, LeagueRepository>();
            services.AddTransient<IPlayerRepository, PlayerRepository>();
            services.AddTransient<ILeagueCommandHandler, LeagueCommandHandler>();
            services.AddTransient<IUserContext, UserContext>();
            services.AddTransient<ILocalStorageService, LocalStorageService>();
            services.AddTransient<IUserAccountRepository, UserAccountRepository>();
            services.AddTransient<IUserAccountCommandHandler, UserAccountCommandHandler>();
            services.AddScoped<UserLoginState>();
            services.AddHttpContextAccessor();
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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}