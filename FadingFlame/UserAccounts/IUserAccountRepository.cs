using System.Threading.Tasks;
using FadingFlame.Repositories;
using MongoDB.Driver;

namespace FadingFlame.UserAccounts
{
    public interface IUserAccountRepository
    {
        Task<UserAccount> Load(string email);
        Task Upsert(UserAccount userAccount);
    }

    public class UserAccountRepository : MongoDbRepositoryBase, IUserAccountRepository
    {
        public UserAccountRepository(MongoClient mongoClient) : base(mongoClient)
        {
        }

        public Task<UserAccount> Load(string email)
        {
            return LoadFirst<UserAccount>(u => u.Email == email);
        }

        public Task Upsert(UserAccount userAccount)
        {
            return Upsert(userAccount, u => u.Email == userAccount.Email);
        }
    }
}