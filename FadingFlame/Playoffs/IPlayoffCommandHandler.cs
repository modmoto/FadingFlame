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
            await _playoffRepository.Delete(_seasonState.CurrentSeason.SeasonId);
            var leagues = await _leagueRepository.LoadForSeason(_seasonState.CurrentSeason.SeasonId);

            var playoffPlayers = new List<PlayerInLeague>();
            for (int index = 0; index < (leagues.Count + 1) / 2; index++)
            {
                if (index == 0)
                {
                    playoffPlayers.Add(leagues[0].Players[0]);
                    playoffPlayers.Add(leagues[0].Players[1]);
                    playoffPlayers.Add(leagues[1].Players[0]);
                    playoffPlayers.Add(leagues[1].Players[1]);

                    if (leagues.Count % 2 == 1)
                    {
                        var place5A = leagues[0].Players[2];
                        var place5B = leagues[1].Players[2];
                        if (place5A.BattlePoints == place5B.BattlePoints)
                        {

                            playoffPlayers.Add(place5A.VictoryPoints > place5B.VictoryPoints ? place5A : place5B);
                        }
                        else
                        {
                            playoffPlayers.Add(place5A.BattlePoints > place5B.BattlePoints ? place5A : place5B);
                        }
                    }
                }
                else if (index == 1)
                {
                    playoffPlayers.Add(leagues[2].Players[0]);
                    playoffPlayers.Add(leagues[2].Players[1]);
                    playoffPlayers.Add(leagues[3].Players[0]);
                    playoffPlayers.Add(leagues[3].Players[1]);
                }
                else
                {
                    playoffPlayers.Add(leagues[index * 2].Players[0]);
                    var leagueBIndex = index * 2 + 1;
                    if (leagues.Count > leagueBIndex)
                    {
                        playoffPlayers.Add(leagues[leagueBIndex].Players[0]);
                    }
                }
            }

            var playoffs = await Playoff.Create(_mmrRepository, _seasonState.CurrentSeason.SeasonId, playoffPlayers);

            await _playoffRepository.Insert(playoffs);

            return playoffs;
        }
    }
}