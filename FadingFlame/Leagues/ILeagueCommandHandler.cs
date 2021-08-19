using System.Collections.Generic;
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
            var currentLeageu = League.Create(season);
            foreach (var player in players)
            {
                currentLeageu.AddPlayer(player);
                counter++;

                if (counter == 10)
                {
                    counter = 0;
                    currentLeageu = League.Create(season);
                    newLeagues.Add(currentLeageu);
                }
            }

            if (currentLeageu.Players.Count != 0)
            {
                newLeagues.Add(currentLeageu);
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