using System;
using System.Linq;
using System.Threading.Tasks;
using FadingFlame.Admin;
using FadingFlame.Leagues;
using FadingFlame.ReadModelBase;

namespace FadingFlame.Matchups
{
    public class SetMissingMatchesToDrawHandler : IAsyncUpdatable
    {
        private readonly ISeasonRepository _seasonRepository;
        private readonly ILeagueRepository _leagueRepository;

        public SetMissingMatchesToDrawHandler(
            ISeasonRepository seasonRepository, 
            ILeagueRepository leagueRepository
            )
        {
            _seasonRepository = seasonRepository;
            _leagueRepository = leagueRepository;
        }

        public async Task<HandlerVersion> Update(HandlerVersion currentVersion)
        {
            var lastRun = DateTime.Parse(currentVersion.Version ?? CreateNowString());
            var season = await _seasonRepository.LoadSeasons();
            var currentSeason = season.First(s => s.IsPubliclyVisible);
            var leagues = await _leagueRepository.LoadForSeason(currentSeason.SeasonId);
            foreach (var league in leagues)
            {
                var allGamesThatShouldBeClosed = league.GameDays.Where(g =>
                {
                    return g.EndDate + TimeSpan.FromDays(14) < lastRun;
                })
                    .SelectMany(g => g.Matchups)
                    .Where(m => !m.IsFinished);
                
                foreach (var matchup in allGamesThatShouldBeClosed)
                {
                    league.SetZeroToZero(matchup.Id);
                    await _leagueRepository.Update(league);
                }
            }

            return HandlerVersion.CreateFrom<SetMissingMatchesToDrawHandler>(CreateNowString());
        }

        private static string CreateNowString()
        {
            return DateTime.UtcNow.ToString("O");
        }

        public int WaitTimeInMs => 60000;
    }
}