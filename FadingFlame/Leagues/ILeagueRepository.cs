using System.Collections.Generic;
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
        Task Udate(League league);
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

        public Task Udate(League league)
        {
            return Upsert(league);
        }
    }
}