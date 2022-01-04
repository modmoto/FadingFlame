using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FadingFlame.Matchups;
using FadingFlame.Repositories;
using MongoDB.Driver;

namespace FadingFlame.Playoffs
{
    public interface IPlayoffRepository
    {
        Task Insert(Playoff playoffs);
        Task<bool> Update(Playoff playoffs);
        Task<Playoff> LoadForSeason(int season);
        Task Delete(int season);
    }

    public class PlayoffRepository : MongoDbRepositoryBase, IPlayoffRepository
    {
        private readonly IMatchupRepository _matchupRepository;

        public PlayoffRepository(MongoClient mongoClient, IMatchupRepository matchupRepository) : base(mongoClient)
        {
            _matchupRepository = matchupRepository;
        }

        public async Task Insert(Playoff playoffs)
        {
            var matchups = playoffs.Rounds.SelectMany(g => g.Matchups).ToList();
            await _matchupRepository.InsertMatches(matchups);
            await base.Insert(playoffs);
        }

        public async Task<bool> Update(Playoff playoffs)
        {
            var result = await UpdateVersionsave(playoffs);
            if (result)
            {
                var matchups = playoffs.Rounds.Where(m => m.Matchups != null).SelectMany(g => g.Matchups).ToList();
                await _matchupRepository.UpdateMatches(matchups);
            }

            return result;
        }

        public async Task<Playoff> LoadForSeason(int season)
        {
            var playoff = await LoadFirst<Playoff>(l => l.Season == season);
            if (playoff == null) return null;

            var matchIds = playoff.Rounds.SelectMany(g => g.MatchupIds).ToList();
            var matches = await _matchupRepository.LoadMatches(matchIds.ToList());
            if (matches.Count == 0) return playoff;

            foreach (var gameDay in playoff.Rounds)
            {
                gameDay.Matchups = new List<Matchup>();
                foreach (var match in gameDay.MatchupIds.Select(id => matches.Single(m => m.Id == id)))
                {
                    gameDay.Matchups.Add(match);
                }
            }

            return playoff;
        }

        public async Task Delete(int season)
        {
            var playoff = await LoadForSeason(season);
            if (playoff == null) return;
            var matchups = playoff.Rounds.SelectMany(g => g.Matchups).Select(m => m.Id).ToList();
            await _matchupRepository.DeleteMatches(matchups);
            await base.Delete<Playoff>(p => p.Season == season);
        }

    }
}