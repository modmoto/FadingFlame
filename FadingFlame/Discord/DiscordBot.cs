using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using FadingFlame.Leagues;
using FadingFlame.Players;

namespace FadingFlame.Discord
{
    public interface IDiscordBot
    {
        Task CreateLeagueChannelsAndTags(List<League> leagues);
        Task SendRequestListChangedToBotsChannel(int pendingChanges);
        Task ConfirmationMessageToUser(string discordTag, bool wasAccepterd);
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
                Intents = DiscordIntents.GuildMembers
                        | DiscordIntents.GuildPresences
                        | DiscordIntents.Guilds
                        | DiscordIntents.DirectMessages
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
                    Task.Delay(50).Wait();
                }
            }

            foreach (var clientGuild in _client.Guilds)
            {
                var guild = clientGuild.Value;
                var leaguesCategory = guild.Channels.FirstOrDefault(c => c.Value.IsCategory && c.Value.Name == "leagues").Value;
                if (leaguesCategory == null)
                {
                    leaguesCategory = await guild.CreateChannelCategoryAsync("leagues");
                    Task.Delay(50).Wait();
                }

                var position = 1;
                foreach (var league in leagues)
                {
                    var players = await _playerRepository.LoadForLeague(league.Players.Select(p => p.Id).ToList(), league.Season);
                    var leagueChannel = guild.Channels.FirstOrDefault(c => c.Value.Type == ChannelType.Text && c.Value.Name == ToLeagueName(league)).Value;
                    if (leagueChannel == null)
                    {
                        leagueChannel = await guild.CreateTextChannelAsync($"league-{league.DivisionId}-{league.Name}", leaguesCategory);
                        Task.Delay(50).Wait();
                    }
                    
                    await leagueChannel.ModifyPositionAsync(position);
                    
                    var role = guild.Roles.FirstOrDefault(r => r.Value.Name == league.DivisionId.ToLower()).Value;
                    if (role == null)
                    {
                        role = await guild.CreateRoleAsync(league.DivisionId.ToLower(), color: LeagueConstants.DiscordColors[position - 1]);
                        Task.Delay(50).Wait();
                    }

                    foreach (var discordMember in guild.Members)
                    {
                        await GrantRoleForLeague(players, discordMember.Value, role);
                    }
                    
                    position++;
                }
            }
        }

        public async Task SendRequestListChangedToBotsChannel(int pendingChanges)
        {
            try
            {
                var channels = _client.Guilds.SelectMany(g => g.Value.Channels).Where(c => c.Value.Name == "bots");
                foreach (var channel in channels)
                {
                    await channel.Value.SendMessageAsync($"{pendingChanges} pending list changes are waiting on the website");
                }
            }
            catch (Exception)
            {
                // ignored, dont care
            }
        }

        public async Task ConfirmationMessageToUser(string discordTag, bool wasAccepterd)
        {
            try
            {
                foreach (var guild in _client.Guilds)
                {
                    var members = await guild.Value.GetAllMembersAsync();
                    var lower = discordTag.ToLower();
                    var discordMember = members.FirstOrDefault(c => $"{c.Username.ToLower()}#{c.Discriminator}" == lower);
                    if (discordMember is not null)
                    {
                        var resul = wasAccepterd ? "accepted, see you on the battlefield =)" : "rejected, reach out to the orga team if you do not approve";
                        await discordMember.SendMessageAsync($"Your list change on fading-flame was {resul}");
                    }
                }
            }
            catch (Exception e)
            {
                // ignored, dont care
            }
        }

        private static async Task GrantRoleForLeague(List<Player> players, DiscordMember member, DiscordRole role)
        {
            var username = member.Username.ToLower() + "#" + member.Discriminator;
            var playerInLeagues = players.FirstOrDefault(p => p.DiscordTag?.ToLower() == username);
            if (playerInLeagues != null)
            {
                var oldLeagueRoles = member.Roles.Where(r => LeagueConstants.Ids.Select(l => l.ToLower()).Contains(r.Name)).ToList();
                foreach (var oldLeagueRole in oldLeagueRoles)
                {
                    await member.RevokeRoleAsync(oldLeagueRole);
                    Task.Delay(50).Wait();
                }
                await member.GrantRoleAsync(role);
                Task.Delay(50).Wait();
            }
        }

        private static string ToLeagueName(League l)
        {
            return $"league-{l.DivisionId.ToLower()}-{l.Name.Replace(" ", "-").ToLower()}";
        }
    }
}