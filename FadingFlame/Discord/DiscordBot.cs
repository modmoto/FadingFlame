using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using FadingFlame.Leagues;
using Microsoft.Extensions.Logging;

namespace FadingFlame.Discord
{
    public class DiscordBot
    {
        private readonly string _token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
        private DiscordClient Client { get; }
        private static DiscordBot _instance;
        private static readonly object Padlock = new();

        public static DiscordBot Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Padlock)
                    {
                        if (_instance == null)
                        {
                            _instance = new DiscordBot();
                        }
                    }
                }
                return _instance;
            }
        }
        
        private DiscordBot()
        {
            var discordConfiguration = new DiscordConfiguration
            {
                Token = _token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug,
            };
            Client = new DiscordClient(discordConfiguration);

            Client.ConnectAsync().Wait();
        }
        
        public async Task CreateLeagueChannels(List<League> leagues)
        {
            var channels = Client.Guilds.SelectMany(g => g.Value.Channels).Where(c => c.Value.Type == ChannelType.Text);
            var regex = new Regex("league-([0-9]{1,2}[a-b])-\\w+");
            var channelsForLeagues = channels.Where(c => regex.IsMatch(c.Value.Name));
            var leagueNames = leagues.Select(l => ToLeagueName(l)).ToList();
            foreach (var channelsForLeague in channelsForLeagues)
            {
                if (!leagueNames.Contains(channelsForLeague.Value.Name))
                {
                    await channelsForLeague.Value.DeleteAsync();
                }
            }

            foreach (var clientGuild in Client.Guilds)
            {
                var leaguesCategory = clientGuild.Value.Channels.FirstOrDefault(c => c.Value.IsCategory && c.Value.Name == "leagues").Value;
                if (leaguesCategory == null)
                {
                    leaguesCategory = await clientGuild.Value.CreateChannelCategoryAsync("leagues");
                }

                var position = 1;
                foreach (var league in leagues)
                {
                    var leagueChannel = clientGuild.Value.Channels.FirstOrDefault(c => c.Value.Type == ChannelType.Text && c.Value.Name == ToLeagueName(league)).Value;
                    if (leagueChannel == null)
                    {
                        leagueChannel = await clientGuild.Value.CreateTextChannelAsync($"league-{league.DivisionId}-{league.Name}", leaguesCategory);
                    }
                    
                    await leagueChannel.ModifyPositionAsync(position);
                    position++;
                }
            }
        }

        private static string ToLeagueName(League l)
        {
            return $"league-{l.DivisionId.ToLower()}-{l.Name.Replace(" ", "-").ToLower()}";
        }
    }
}