using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FadingFlame.Leagues;

namespace FadingFlame.Playoffs
{
    public interface IPlayoffCommandHandler
    {
        Task<Playoff> CreatePlayoffs();
    }

    public class PlayoffCommandHandler : IPlayoffCommandHandler
    {
        private readonly ILeagueRepository _leagueRepository;
        private readonly IPlayoffRepository _playoffRepository;

        public PlayoffCommandHandler(ILeagueRepository leagueRepository, IPlayoffRepository playoffRepository)
        {
            _leagueRepository = leagueRepository;
            _playoffRepository = playoffRepository;
        }

        public async Task<Playoff> CreatePlayoffs()
        {
            var leagues = await _leagueRepository.LoadForSeason(Season.Current);

            var firstPlaces = leagues.Select(l => l.Players.First()).ToList();

            var sortedFirstPlaces = new List<PlayerInLeague>();
            for (int index = 0; index < firstPlaces.Count / 2; index++)
            {
                var player1 = firstPlaces[index];
                var player2 = firstPlaces[index + firstPlaces.Count / 2];

                sortedFirstPlaces.Add(player1);
                sortedFirstPlaces.Add(player2);
            }

            var playoffs = Playoff.Create(Season.Current, sortedFirstPlaces);

            await _playoffRepository.Insert(playoffs);

            return playoffs;
        }
    }
}