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
        Task<Player> LoadByEmail(string accountEmail);
        Task<List<Player>> PlayersThatEnlisted();
        Task<List<Player>> LoadForLeague(List<ObjectId> playerIds);
        Task<List<Player>> LoadAllWithoutList();
        Task<List<Player>> LoadAllWitList();
        Task DeleteAllWithMail(string notfoundMail);
        Task<List<Player>> FindAllWithFakeMail(string notfoundMail);
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

        public Task<Player> LoadByEmail(string accountEmail)
        {
            return LoadFirst<Player>(p => p.AccountEmail == accountEmail);
        }

        public Task<List<Player>> PlayersThatEnlisted()
        {
            return LoadAll<Player>(p => p.Army != null);
        }

        public Task<List<Player>> LoadForLeague(List<ObjectId> playerIds)
        {
            return LoadAll<Player>(p => playerIds.Contains(p.Id));
        }

        public Task<List<Player>> LoadAllWithoutList()
        {
            return LoadAll<Player>(p => p.Army == null);
        }

        public Task<List<Player>> LoadAllWitList()
        {
            return LoadAll<Player>(p => p.Army != null);
        }

        public Task DeleteAllWithMail(string notfoundMail)
        {
            return DeleteMultiple<Player>(p => p.AccountEmail == notfoundMail);
        }

        public Task<List<Player>> FindAllWithFakeMail(string notfoundMail)
        {
            return LoadAll<Player>(p => p.AccountEmail == notfoundMail);
        }
    }
}