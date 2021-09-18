using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public LeagueRepository(MongoClient mongoClient) : base(mongoClient)
        {
        }

        public Task<List<League>> LoadForSeason(int season)
        {
            return LoadAll<League>(l => l.Season == season);
        }

        public Task<League> Load(ObjectId id)
        {
            return LoadFirst<League>(id);
        }

        public Task Insert(List<League> newLeagues)
        {
            return base.Insert(newLeagues);
        }

        public Task DeleteForSeason(int season)
        {
            return DeleteMultiple<League>(l => l.Season == season);
        }

        public Task Update(League league)
        {
            return Upsert(league);
        }

        public Task<List<League>> LoadLeaguesForPlayer(ObjectId playerId)
        {
            return LoadAll<League>(l => l.Players.Any(r => r.Id == playerId));
        }
    }
}