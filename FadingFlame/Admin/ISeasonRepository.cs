using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FadingFlame.Repositories;
using MongoDB.Driver;

namespace FadingFlame.Admin
{
    public interface ISeasonRepository
    {
        Task<List<Season>> LoadSeasons();
        Task Update(Season season);
    }

    public class SeasonRepository : MongoDbRepositoryBase, ISeasonRepository
    {
        public SeasonRepository(MongoClient mongoClient) : base(mongoClient)
        {
        }

        public async Task<List<Season>> LoadSeasons()
        {
            var loadSeasons = await LoadAll<Season>();
            return loadSeasons.OrderByDescending(s => s.SeasonId).ToList();
        }

        public Task Update(Season season)
        {
            return Upsert(season, s => s.SeasonId == season.SeasonId);
        }
    }
}