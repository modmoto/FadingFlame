using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FadingFlame.Players;

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

        public LeagueCommandHandler(ILeagueRepository leagueRepository, IPlayerRepository playerRepository)
        {
            _leagueRepository = leagueRepository;
            _playerRepository = playerRepository;
        }

        public async Task CreateLeagues()
        {
            // Todo add season
            var season = 1;

            // var leagues = await _leagueRepository.LoadForSeason(season - 1);

            var players = await _playerRepository.LoadAll();
            int counter = 0;
            var newLeagues = new List<League>();

            var names = new List<string>()
            {
                "Sunna",
                "Sigmar",
                "Nagash",
                "Mannfred",
                "Thorek",
                "Grungi",
                "Gork",
                "Mork",
                "Teclis",
                "Morathi",
                "Kroak",
                "Hellebron",
                "Settra",
                "Karl",
                "Gothrik",
                "Felix",
                "Dummy",
                "Dummy",
                "Dummy",
                "Dummy",
                "Dummy",
                "Dummy",
                "Dummy",
                "Dummy",
                "Dummy",
                "Dummy",
                "Dummy",
                "Dummy",
                "Dummy",
                "Dummy",
                "Dummy",
                "Dummy",
                "Dummy",
                "Dummy",
            };
            
            var ids = new List<string>()
            {
                "1A",
                "1B",
                "2A",
                "2B",
                "3A",
                "3B",
                "4A",
                "4B",
                "5A",
                "5B",
                "6A",
                "6B",
                "7A",
                "7B",
                "8A",
                "8B",
                "9A",
                "9B",
                "10A",
                "10B",
                "11A",
                "11B",
                "12A",
                "12B",
                "13A",
                "13B",
                "14A",
                "14B",
                "15A",
                "15B",
                "16A",
                "16B",
                "17A",
                "17B",
                "18A",
                "18B",
            };
            
            var currentLeageu = League.Create(season, ids.First(), names.First());
            newLeagues.Add(currentLeageu);
            for (var index = 1; index < players.Count; index++)
            {
                var player = players[index];
                currentLeageu.AddPlayer(player);
                counter++;

                if (counter == 6)
                {
                    var newLeaguesCount = newLeagues.Count;
                    currentLeageu = League.Create(season, ids[newLeaguesCount], names[newLeaguesCount]);
                    newLeagues.Add(currentLeageu);
                    counter = 0;
                }
            }

            await _leagueRepository.Insert(newLeagues);
            //
            // if (leagues.Count == 0)
            // {
            //
            // }
            // else
            // {
            //     // Todo promote and demote
            // }
        }
    }
}