using System;
using FadingFlame.Discord;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace FadingFlame
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var discordBot = DiscordBot.Instance;
            Console.WriteLine($"bot {discordBot}");
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}