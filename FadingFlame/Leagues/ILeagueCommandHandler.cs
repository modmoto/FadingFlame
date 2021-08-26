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
            // Todo add season
            var season = 1;

            await _leagueRepository.DeleteForSeason(season);

            var players = await _playerRepository.LoadAll();
            int counter = 0;
            var newLeagues = new List<League>();

            var names = new List<string>()
            {
                "Sunna",
                "Sigmar",
                "Nagash",
                "Von Carstein",
                "Thorek",
                "Grungi",
                "Gork",
                "Mork",
                "Teclis",
                "Morathi",
                "Kroak",
                "Hellebron",
                "Settra",
                "Karl Franz",
                "Gotrek",
                "Felix",
                "Grimgor",
                "Thanquol",
                "Snikch",
                "Golgfag",
                "Kroq-gar",
                "Oxyotl",
                "Hellsnicht",
                "Green Knight",
                "Be'lakor",
                "Archaon",
                "Ariel",
                "Araloth",
                "Imrik",
                "Tzarina Katarin",
                "Tzar Boris",
                "Settra",
                "The Red Duke",
                "Abhorash",
                "Greasus Goldtooth",
                "Mazdamundi",
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

            var dateTimeOffset = new DateTimeOffset();
            var startDate = dateTimeOffset.AddDays(14).AddMonths(9).AddYears(2020);
            var league = League.Create(season, startDate, ids.First(), names.First());
            for (var index = 0; index < players.Count; index++)
            {
                var player = players[index];
                league.AddPlayer(player);

                if (league.IsFull)
                {
                    newLeagues.Add(league);
                    league = League.Create(season, startDate, ids[newLeagues.Count], names[newLeagues.Count]);
                }
            }

            await _leagueRepository.Insert(newLeagues);
            return newLeagues;
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