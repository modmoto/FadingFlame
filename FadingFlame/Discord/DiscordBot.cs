using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using FadingFlame.Leagues;
using FadingFlame.Players;
using Microsoft.Extensions.Logging;

namespace FadingFlame.Discord
{
    public interface IDiscordBot
    {
        Task CreateLeagueChannelsAndTags(List<League> leagues);
    }

    public class DiscordBot : IDiscordBot
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly DiscordClient _client;

        public DiscordBot(string token, IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
            var discordConfiguration = new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug
            };
            
            _client = new DiscordClient(discordConfiguration);
            _client.ConnectAsync().Wait();
        }
        
        public async Task CreateLeagueChannelsAndTags(List<League> leagues)
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
                    await Task.Delay(50);
                }
            }

            foreach (var clientGuild in _client.Guilds)
            {
                var leaguesCategory = clientGuild.Value.Channels.FirstOrDefault(c => c.Value.IsCategory && c.Value.Name == "leagues").Value;
                if (leaguesCategory == null)
                {
                    leaguesCategory = await clientGuild.Value.CreateChannelCategoryAsync("leagues");
                    await Task.Delay(50);
                }

                var position = 1;
                foreach (var league in leagues)
                {
                    var players = await _playerRepository.LoadForLeague(league.Players.Select(p => p.Id).ToList());
                    var leagueChannel = clientGuild.Value.Channels.FirstOrDefault(c => c.Value.Type == ChannelType.Text && c.Value.Name == ToLeagueName(league)).Value;
                    if (leagueChannel == null)
                    {
                        leagueChannel = await clientGuild.Value.CreateTextChannelAsync($"league-{league.DivisionId}-{league.Name}", leaguesCategory);
                        await Task.Delay(50);
                    }
                    
                    await leagueChannel.ModifyPositionAsync(position);
                    
                    var role = clientGuild.Value.Roles.FirstOrDefault(r => r.Value.Name == league.DivisionId.ToLower()).Value;
                    if (role == null)
                    {
                        role = await clientGuild.Value.CreateRoleAsync(league.DivisionId.ToLower(), color: LeagueConstants.DiscordColors[position - 1]);
                        await Task.Delay(50);
                    }

                    var owner = clientGuild.Value.Owner;
                    await GrantRoleForLeague(players, owner, role);
                    foreach (var discordMember in clientGuild.Value.Members)
                    {
                        await GrantRoleForLeague(players, discordMember.Value, role);
                    }
                    
                    position++;
                }
            }
        }

        private static async Task GrantRoleForLeague(List<Player> players, DiscordMember member, DiscordRole role)
        {
            var username = member.Username.ToLower() + "#" + member.Discriminator;
            var playerInLeagues = players.FirstOrDefault(p => p.DiscordTag?.ToLower() == username);
            if (playerInLeagues != null)
            {
                var oldLeagueRoles = member.Roles.Where(r => LeagueConstants.Ids.Select(l => l.ToLower()).Contains(r.Name));
                foreach (var oldLeagueRole in oldLeagueRoles)
                {
                    await member.RevokeRoleAsync(oldLeagueRole);
                    await Task.Delay(50);
                }
                await member.GrantRoleAsync(role);
                await Task.Delay(50);
            }
        }

        private static string ToLeagueName(League l)
        {
            return $"league-{l.DivisionId.ToLower()}-{l.Name.Replace(" ", "-").ToLower()}";
        }
    }
}