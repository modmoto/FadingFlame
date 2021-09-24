using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FadingFlame.Matchups;
using FadingFlame.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FadingFlame.Leagues
{
    public interface ILeagueRepository
    {
        Task<List<League>> LoadForSeason(int season);
        Task<League> Load(ObjectId id);
        Task Insert(List<League> newLeagues);
        Task DeleteForSeason(int season);
        Task Update(League league);
        Task<List<League>> LoadLeaguesForPlayer(ObjectId playerId);
    }

    public class LeagueRepository : MongoDbRepositoryBase, ILeagueRepository
    {
        private readonly IMatchupRepository _matchupRepository;

        public LeagueRepository(MongoClient mongoClient, IMatchupRepository matchupRepository) : base(mongoClient)
        {
            _matchupRepository = matchupRepository;
        }

        public Task<List<League>> LoadForSeason(int season)
        {
            return LoadAll<League>(l => l.Season == season);
        }

        public async Task<League> Load(ObjectId id)
        {
            var league = await LoadFirst<League>(id);
            var matchIds = league.GameDays.SelectMany(g => g.MatchupIds);
            var matches = await _matchupRepository.LoadMatches(matchIds.ToList());

            foreach (var gameDay in league.GameDays)
            {
                var matchesInGameDay = matches.Where(m => gameDay.MatchupIds.Contains(m.Id)).ToList();
                gameDay.Matchups = matchesInGameDay;
            }

            return league;
        }

        public async Task Insert(List<League> newLeagues)
        {
            var matchups = newLeagues.SelectMany(l => l.GameDays.SelectMany(g => g.Matchups)).ToList();
            await _matchupRepository.InsertMatches(matchups);    
            await base.Insert(newLeagues);
        }

        public async Task DeleteForSeason(int season)
        {
            var leagues = await LoadForSeason(season);
            var matchups = leagues.SelectMany(l => l.GameDays.SelectMany(g => g.MatchupIds)).ToList();
            await _matchupRepository.DeleteMatches(matchups);
            await DeleteMultiple<League>(l => l.Season == season);
        }

        public async Task Update(League league)
        {
            var matchups = league.GameDays.SelectMany(g => g.Matchups).ToList();
            await _matchupRepository.UpdateMatches(matchups);
            await Upsert(league);
        }

        public Task<List<League>> LoadLeaguesForPlayer(ObjectId playerId)
        {
            return LoadAll<League>(l => l.Players.Any(r => r.Id == playerId));
        }
    }
}