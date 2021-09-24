using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FadingFlame.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FadingFlame.Matchups
{
    public interface IMatchupRepository
    {
        Task<List<Matchup>> LoadMatches(List<ObjectId> matchIds);
        Task InsertMatches(List<Matchup> matchups);
        Task DeleteMatches(List<ObjectId> matchups);
        Task UpdateMatches(List<Matchup> matchups);
    }

    public class MatchupRepository : MongoDbRepositoryBase, IMatchupRepository
    {
        public MatchupRepository(MongoClient mongoClient) : base(mongoClient)
        {
        }
        public Task<List<Matchup>> LoadMatches(List<ObjectId> matchIds)
        {
            return LoadAll<Matchup>(m => matchIds.Contains(m.Id));
        }

        public Task InsertMatches(List<Matchup> matchups)
        {
            if (matchups.Count == 0) return Task.CompletedTask;
            return Insert(matchups);
        }

        public Task DeleteMatches(List<ObjectId> matchups)
        {
            return DeleteMultiple<Matchup>(matchups);
        }

        public Task UpdateMatches(List<Matchup> matchups)
        {
            return UpsertMany(matchups);
        }
    }
}