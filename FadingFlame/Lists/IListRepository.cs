using System.Collections.Generic;
using System.Threading.Tasks;
using FadingFlame.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FadingFlame.Lists
{
    public interface IListRepository
    {
        Task<Army> Load(ObjectId armyId);
        Task Insert(Army army);
        Task Update(Army army);
        Task<List<Army>> LoadWithPendingChanges();
    }

    public class ListRepository : MongoDbRepositoryBase, IListRepository
    {

        public ListRepository(MongoClient mongoClient) : base(mongoClient)
        {
        }

        public Task<Army> Load(ObjectId armyId)
        {
            return LoadFirst<Army>(armyId);
        }

        public Task Insert(Army army)
        {
            return base.Insert(army);
        }

        public Task Update(Army army)
        {
            return Upsert(army);
        }

        public Task<List<Army>> LoadWithPendingChanges()
        {
            return LoadAll<Army>(army => army.List1.ProposedListChange != default || army.List2.ProposedListChange != default);
        }
    }
}