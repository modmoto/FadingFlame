using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FadingFlame.Leagues;
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
        
        public async Task<List<PlayerRankingReadModel>> LoadAllRanked()
        {
            var loadAllRanked = await LoadAll<PlayerRankingReadModel>(r => r.Id != LeagueConstants.FreeWinPlayer);
            return loadAllRanked.OrderByDescending(i => i.Mmr).ToList();
        }

        public Task Upsert(List<PlayerRankingReadModel> rankingModels)
        {
            return UpsertMany(rankingModels);
        }
    }
}