using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FadingFlame.Players;

namespace FadingFlame.Leagues
{
    public interface ILeagueCommandHandler
    {
        Task<List<League>> CreateLeagues();
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

        public async Task<List<League>> CreateLeagues()
        {
            await _leagueRepository.DeleteForSeason(Season.Current);

            var players = await _playerRepository.LoadAll();
            var newLeagues = new List<League>();

            var dateTimeOffset = new DateTimeOffset();
            var startDate = dateTimeOffset.AddDays(14).AddMonths(9).AddYears(2020);
            var league = League.Create(Season.Current, startDate, LeagueConstants.Ids.First(), LeagueConstants.Names.First());
            for (var index = 0; index < players.Count; index++)
            {
                var player = players[index];
                league.AddPlayer(player);

                if (league.IsFull)
                {
                    newLeagues.Add(league);
                    league = League.Create(Season.Current, startDate, LeagueConstants.Ids[newLeagues.Count], LeagueConstants.Names[newLeagues.Count]);
                }

                player.ResetLists();
                await _playerRepository.Update(player);
            }

            await _leagueRepository.Insert(newLeagues);
            return newLeagues;
        }
    }
}