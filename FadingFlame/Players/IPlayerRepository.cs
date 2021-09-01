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
        Task Update(Player player);
        Task Insert(Player player);
        Task<Player> LoadByAccountName(string email);
        Task<List<Player>> PlayersThatEnlisted();
        Task<List<Player>> LoadForLeague(List<ObjectId> playerIds);
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

        public Task Update(Player player)
        {
            return Upsert(player);
        }

        public Task Insert(Player player)
        {
            return base.Insert(player);
        }

        public Task<Player> LoadByAccountName(string email)
        {
            return LoadFirst<Player>(p => p.AccountName == email);
        }

        public Task<List<Player>> PlayersThatEnlisted()
        {
            return LoadAll<Player>(p => p.Army != null);
        }

        public Task<List<Player>> LoadForLeague(List<ObjectId> playerIds)
        {
            return LoadAll<Player>(p => playerIds.Contains(p.Id));
        }
    }
}