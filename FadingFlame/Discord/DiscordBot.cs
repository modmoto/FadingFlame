using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using FadingFlame.Leagues;
using FadingFlame.Players;

namespace FadingFlame.Discord
{
    public interface IDiscordBot
    {
        Task<List<PlayerAndLeagueError>> SetLeagueTagsOnPlayers(List<League> leagues);
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

        public async Task<List<PlayerAndLeagueError>> SetLeagueTagsOnPlayers(List<League> leagues)
        {
            var playerInLeagues = leagues.SelectMany(l => l.Players);
            var players = await _playerRepository.LoadForLeague(playerInLeagues.Select(p => p.Id).ToList());
            var notFoundPlayers = new List<PlayerAndLeagueError>();
            foreach (var clientGuild in _client.Guilds)
            {
                var guild = clientGuild.Value;

                foreach (var player in players)
                {
                    var member = guild.Members.FirstOrDefault(member => member.Value.Username?.ToLower() + "#" + member.Value.Discriminator == player.DiscordTag?.ToLower()).Value;
                    var leagueOfPlayer = leagues.Single(l => l.Players.Select(p => p.Id).Contains(player.Id));
                    if (member == null)
                    {
                        notFoundPlayers.Add(new PlayerAndLeagueError(player.DisplayName, player.DiscordTag, leagueOfPlayer.DivisionId));
                    }
                    else
                    {
                        var role = guild.Roles.FirstOrDefault(r => r.Value.Name == leagueOfPlayer.DivisionId).Value;
                        var oldLeagueRoles = member.Roles.Where(r => LeagueConstants.Ids.Contains(r.Name)).ToList();
                        foreach (var oldLeagueRole in oldLeagueRoles)
                        {
                            await member.RevokeRoleAsync(oldLeagueRole);
                            Task.Delay(50).Wait();
                        }
                        await member.GrantRoleAsync(role);
                        Task.Delay(50).Wait();
                    }
                }
            }

            return notFoundPlayers;
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
            catch (Exception)
            {
                // ignored, dont care
            }
        }
    }

    public class PlayerAndLeagueError
    {
        public string DisplayName { get; }
        public string DiscordTag { get; }
        public string LeagueTag { get; }

        public PlayerAndLeagueError(string displayName, string discordTag, string leagueTag)
        {
            DisplayName = displayName;
            DiscordTag = discordTag;
            LeagueTag = leagueTag;
        }
    }
}