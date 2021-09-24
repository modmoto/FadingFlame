using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FadingFlame.Admin;
using FadingFlame.Players;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
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
            var notFoundPlayers = new List<string>();

            var serviceValues = GetSheetsService().Spreadsheets.Values;
            
            for (int leagueIndex = 0; leagueIndex < 19; leagueIndex++)
            {
                var stringsOfExcel = await ReadAsync(serviceValues, leagueIndex);
                var league = League.Create(currentSeason.SeasonId, currentSeason.StartDate, LeagueConstants.Ids[leagueIndex], LeagueConstants.Names[leagueIndex]);
                for (int playerIndex = 0; playerIndex < 6; playerIndex++)
                {
                    var index = playerIndex + 1 * playerIndex;
                    var discordTag = stringsOfExcel[index];
                    var name = stringsOfExcel[index + 1];

                    var foundPlayer = players.FirstOrDefault(p => 
                        p.DiscordTag?.ToLower() == discordTag.ToLower()
                        || p.DisplayName.ToLower() == name?.ToLower());

                    if (foundPlayer != null)
                    {
                        league.AddPlayer(foundPlayer);
                    }
                    else
                    {
                        var player = Player.Create($"MISSING_{discordTag}", _notfoundMail);
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
        
        private static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        private const string SpreadsheetId = "1WY2I-Nl0n2HgWp2MMjnEpRQ3DmbErQFsNZvR8DMgJeM";
        private const string GoogleCredentialsFileName = "fading-flame-babbfd3eb7b2.json";
       
        private SheetsService GetSheetsService()
        {            
            using (var stream = new FileStream($"{_hostEnvironment.WebRootPath}/{GoogleCredentialsFileName}", FileMode.Open, FileAccess.Read))
            {
                var serviceInitializer = new BaseClientService.Initializer
                {
                    HttpClientInitializer = GoogleCredential.FromStream(stream).CreateScoped(Scopes)
                };
                return new SheetsService(serviceInitializer);
            }
        }
        
        private async Task<List<string>> ReadAsync(SpreadsheetsResource.ValuesResource valuesResource, int index)
        {
            var response = await valuesResource.Get(SpreadsheetId, $"{_chars[index]}2:{_chars[index]}14").ExecuteAsync();
            var values = response.Values;
            var strings = values.Select(v => v.FirstOrDefault()?.ToString()).ToList();
            if (strings.Count == 11)
            {
                strings.Add(null);
            }
            return strings.ToList();
        }

        private List<string> _chars = new()
        {
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U",
            "V", "W", "X"
        };
    }
}