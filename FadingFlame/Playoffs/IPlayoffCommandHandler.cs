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
            // Todo add season
            var season = 1;

            var leagues = await _leagueRepository.LoadForSeason(season);

            var firstPlaces = leagues.Select(l => l.Players.First()).ToList();

            var playoffs = Playoff.Create(season, firstPlaces);

            await _playoffRepository.Insert(playoffs);

            return playoffs;
        }
    }
}