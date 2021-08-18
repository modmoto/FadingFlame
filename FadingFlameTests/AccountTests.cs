using System.Threading.Tasks;
using FadingFlame.Players;
using FadingFlame.UserAccounts;
using MongoDB.Bson;
using Moq;
using NUnit.Framework;

namespace FadingFlameTests
{
    [TestFixture]
    public class AccountTests : IntegrationTestBase
    {
        [Test]
        public async Task AddAccount()
        {
            var userAccountRepository = new UserAccountRepository(MongoClient);
            var userAccountCommandHandler = new UserAccountCommandHandler(
                new Mock<ILocalStorageService>().Object,
                userAccountRepository,
                new PlayerRepository(MongoClient),
                new Mock<IUserContext>().Object);

            var email = "simon@123.de";
            var registerModel = new RegisterModel
            {
                Email = email,
                DisplayName = "modmoto",
                Password = "secret"
            };
            await userAccountCommandHandler.Register(registerModel);

            var userAccount = await userAccountRepository.Load(email);
            var player = await new PlayerRepository(MongoClient).Load(userAccount.PlayerId);

            Assert.AreEqual(userAccount.Email, email);
            Assert.AreEqual(userAccount.Password, "secret");
            Assert.AreNotEqual(userAccount.PlayerId, default(ObjectId));
            Assert.AreEqual(userAccount.PlayerId, userAccount.PlayerId);
            Assert.AreEqual(player.DisplayName, "modmoto");
        }
    }
}