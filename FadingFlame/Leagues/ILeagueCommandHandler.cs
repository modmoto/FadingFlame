using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FadingFlame.Admin;
using FadingFlame.Players;
using Microsoft.AspNetCore.Hosting;

namespace FadingFlame.Leagues
{
    public interface ILeagueCommandHandler
    {
        Task CreateLeagues();
    }

    public class LeagueCommandHandler : ILeagueCommandHandler
    {
        private readonly ILeagueRepository _leagueRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly SeasonState _seasonState;
        private string _notfoundMail = "NotFoundPlayer@lel.de";

        public LeagueCommandHandler(
            ILeagueRepository leagueRepository,
            IPlayerRepository playerRepository,
            IWebHostEnvironment hostEnvironment,
            SeasonState seasonState)
        {
            _leagueRepository = leagueRepository;
            _playerRepository = playerRepository;
            _hostEnvironment = hostEnvironment;
            _seasonState = seasonState;
        }

        public async Task CreateLeagues()
        {
            _seasonState.SetNotFoundPlayer(new List<string>());
            var currentSeason = _seasonState.CurrentSeason;
            await _leagueRepository.DeleteForSeason(currentSeason.SeasonId);

            var players = await _playerRepository.LoadAll();
            await _playerRepository.DeleteAllWithMail(_notfoundMail);
            var newLeagues = new List<League>();

            var allLines = await File.ReadAllLinesAsync($"{_hostEnvironment.WebRootPath}/leagues.csv");
            var notFoundPlayers = new List<string>();
            var commaDelimitedValues = allLines.Select(l => l.Split(",")).ToList();
            for (int leagueIndex = 0; leagueIndex < 19; leagueIndex++)
            {
                var league = League.Create(currentSeason.SeasonId, currentSeason.StartDate, LeagueConstants.Ids[leagueIndex], LeagueConstants.Names[leagueIndex]);
                for (int playerIndex = 0; playerIndex < 6; playerIndex++)
                {
                    var index = playerIndex + 1 * playerIndex;
                    var discordTag = commaDelimitedValues[index][leagueIndex];
                    var name = commaDelimitedValues[index + 1][leagueIndex];

                    var foundPlayer = players.FirstOrDefault(p => 
                        p.DiscordTag?.ToLower() == discordTag.ToLower()
                        || p.DisplayName.ToLower() == name.ToLower());

                    if (foundPlayer != null)
                    {
                        league.AddPlayer(foundPlayer);
                    }
                    else
                    {
                        var player = Player.Create($"missing_{discordTag}", _notfoundMail);
                        notFoundPlayers.Add($"{discordTag},{name}");
                        await _playerRepository.Insert(player);
                        league.AddPlayer(player);
                    }
                }
                
                newLeagues.Add(league);
            }

            _seasonState.SetNotFoundPlayer(notFoundPlayers);
            
            await _leagueRepository.Insert(newLeagues);
        }
    }
}