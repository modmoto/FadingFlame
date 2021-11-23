using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FadingFlame.Admin;
using FadingFlame.Leagues;
using FadingFlame.Players;

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
        private readonly IMmrRepository _mmrRepository;
        private readonly SeasonState _seasonState;

        public PlayoffCommandHandler(
            ILeagueRepository leagueRepository,
            IPlayoffRepository playoffRepository,
            SeasonState seasonState, 
            IMmrRepository mmrRepository)
        {
            _leagueRepository = leagueRepository;
            _playoffRepository = playoffRepository;
            _seasonState = seasonState;
            _mmrRepository = mmrRepository;
        }

        public async Task<Playoff> CreatePlayoffs()
        {
            var leagues = await _leagueRepository.LoadForSeason(_seasonState.CurrentSeason.SeasonId);

            var firstPlaces = leagues.Select(l => l.Players.First()).ToList();

            var sortedFirstPlaces = new List<PlayerInLeague>();
            for (int index = 0; index < firstPlaces.Count / 2; index++)
            {
                var player1 = firstPlaces[index];
                var player2 = firstPlaces[index + firstPlaces.Count / 2];

                sortedFirstPlaces.Add(player1);
                sortedFirstPlaces.Add(player2);
            }

            var playoffs = await Playoff.Create(_mmrRepository, _seasonState.CurrentSeason.SeasonId, sortedFirstPlaces);

            await _playoffRepository.Insert(playoffs);

            return playoffs;
        }
    }
}