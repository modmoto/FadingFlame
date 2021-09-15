using System;
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
            await _leagueRepository.DeleteForSeason(LeagueConstants.CurrentSeason);

            var players = await _playerRepository.LoadAll();
            var newLeagues = new List<League>();

            var league = League.Create(LeagueConstants.CurrentSeason, LeagueConstants.StartDate, LeagueConstants.Ids.First(), LeagueConstants.Names.First());
            for (var index = 0; index < players.Count; index++)
            {
                var player = players[index];
                league.AddPlayer(player);

                if (league.IsFull)
                {
                    newLeagues.Add(league);
                    league = League.Create(LeagueConstants.CurrentSeason, LeagueConstants.StartDate, LeagueConstants.Ids[newLeagues.Count], LeagueConstants.Names[newLeagues.Count]);
                }

                player.ResetLists();
                await _playerRepository.Update(player);
            }

            await _leagueRepository.Insert(newLeagues);
        }
    }
}