using System.Collections.Generic;
using System.Threading.Tasks;
using FadingFlame.Repositories;
using MongoDB.Driver;

namespace FadingFlame.GlobalLadder
{
    public interface IRankingReadmodelRepository
    {
        Task<List<PlayerRankingReadModel>> LoadAllRanked();
        Task Upsert(List<PlayerRankingReadModel> rankingModels);
    }

    public class RankingReadmodelRepository : MongoDbRepositoryBase, IRankingReadmodelRepository
    {
        public RankingReadmodelRepository(MongoClient mongoClient) : base(mongoClient)
        {
        }
        
        public Task<List<PlayerRankingReadModel>> LoadAllRanked()
        {
            return LoadAll<PlayerRankingReadModel>();
        }

        public Task Upsert(List<PlayerRankingReadModel> rankingModels)
        {
            return UpsertMany(rankingModels);
        }
    }
}