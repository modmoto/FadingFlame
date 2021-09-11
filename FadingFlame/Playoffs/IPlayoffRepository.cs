using System.Threading.Tasks;
using FadingFlame.Repositories;
using MongoDB.Driver;

namespace FadingFlame.Playoffs
{
    public interface IPlayoffRepository
    {
        Task Insert(Playoff playoffs);
        Task Update(Playoff playoffs);
    }

    public class PlayoffRepository : MongoDbRepositoryBase, IPlayoffRepository
    {
        public Task Insert(Playoff playoffs)
        {
            return base.Insert(playoffs);
        }

        public Task Update(Playoff playoffs)
        {
            return Upsert(playoffs);
        }

        public PlayoffRepository(MongoClient mongoClient) : base(mongoClient)
        {
        }
    }
}