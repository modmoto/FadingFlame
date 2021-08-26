using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using FadingFlame.Leagues;
using Microsoft.Extensions.Logging;

namespace FadingFlame.Discord
{
    public interface IDiscordBot
    {
        Task CreateLeagueChannels(List<League> leagues);
    }

    public class DiscordBot : IDiscordBot
    {
        private readonly DiscordClient _client;

        public DiscordBot(string token)
        {
            var discordConfiguration = new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug,
            };
            
            _client = new DiscordClient(discordConfiguration);
            _client.ConnectAsync().Wait();
        }
        
        public async Task CreateLeagueChannels(List<League> leagues)
        {
            var channels = _client.Guilds.SelectMany(g => g.Value.Channels).Where(c => c.Value.Type == ChannelType.Text);
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

            var discordColors = new List<DiscordColor>()
            {
                DiscordColor.Aquamarine,
                DiscordColor.Azure,
                DiscordColor.Blue,
                DiscordColor.Blurple,
                DiscordColor.Brown,
                DiscordColor.Chartreuse,
                DiscordColor.Cyan,
                DiscordColor.Gold,
                DiscordColor.Goldenrod,
                DiscordColor.Gray,
                DiscordColor.Grayple,
                DiscordColor.Green,
                DiscordColor.Lilac,
                DiscordColor.Magenta,
                DiscordColor.Orange,
                DiscordColor.Purple,
                DiscordColor.Red,
                DiscordColor.Rose,
                DiscordColor.Rose,
                DiscordColor.Sienna,
                DiscordColor.Teal,
                DiscordColor.Turquoise,
                DiscordColor.Violet,
                DiscordColor.Wheat
            };

            foreach (var clientGuild in _client.Guilds)
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

                    var role = clientGuild.Value.Roles.FirstOrDefault(r => r.Value.Name == league.DivisionId.ToLower()).Value;
                    if (role == null)
                    {
                        role = await clientGuild.Value.CreateRoleAsync(league.DivisionId.ToLower(), color: discordColors[position - 1]);
                    }

                    foreach (var discordMember in clientGuild.Value.Members)
                    {
                        var username = discordMember.Value.Username.ToLower();
                        var playerInLeagues = leagues.SelectMany(l => l.Players).FirstOrDefault(p => p.DiscordTag?.Split("#")[0].ToLower() == username);
                        if (playerInLeagues != null)
                        {
                            await discordMember.Value.GrantRoleAsync(role);
                        }
                    }
                    
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