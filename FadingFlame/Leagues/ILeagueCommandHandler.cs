using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FadingFlame.Admin;
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
        private readonly SeasonState _seasonState;

        public LeagueCommandHandler(
            ILeagueRepository leagueRepository,
            IPlayerRepository playerRepository,
            SeasonState seasonState)
        {
            _leagueRepository = leagueRepository;
            _playerRepository = playerRepository;
            _seasonState = seasonState;
        }

        public async Task CreateLeagues()
        {
            var currentSeason = _seasonState.CurrentSeason;
            await _leagueRepository.DeleteForSeason(currentSeason.SeasonId);

            var players = await _playerRepository.LoadAll();
            var newLeagues = new List<League>();

            var league = League.Create(currentSeason.SeasonId, currentSeason.StartDate, LeagueConstants.Ids.First(), LeagueConstants.Names.First());
            for (var index = 0; index < players.Count; index++)
            {
                var player = players[index];
                league.AddPlayer(player);

                if (league.IsFull)
                {
                    newLeagues.Add(league);
                    league = League.Create(currentSeason.SeasonId, currentSeason.StartDate, LeagueConstants.Ids[newLeagues.Count], LeagueConstants.Names[newLeagues.Count]);
                }
            }

            await _leagueRepository.Insert(newLeagues);
        }
    }
}