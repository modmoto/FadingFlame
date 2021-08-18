using System.Collections.Generic;
using System.Threading.Tasks;
using FadingFlame.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FadingFlame.Players
{
    public interface IPlayerRepository
    {
        Task<List<Player>> LoadAll();
        Task<Player> Load(ObjectId id);
        Task Udate(Player player);
        Task Insert(Player player);
    }

    public class PlayerRepository : MongoDbRepositoryBase, IPlayerRepository
    {
        public PlayerRepository(MongoClient mongoClient) : base(mongoClient)
        {
        }

        public Task<List<Player>> LoadAll()
        {
            return LoadAll<Player>();
        }

        public Task<Player> Load(ObjectId id)
        {
            return LoadFirst<Player>(id);
        }

        public Task Udate(Player player)
        {
            return Upsert(player);
        }

        public Task Insert(Player player)
        {
            return base.Insert(player);
        }
    }
}