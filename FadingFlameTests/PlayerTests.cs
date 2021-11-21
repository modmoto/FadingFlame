using System.Linq;
using System.Threading.Tasks;
using FadingFlame.Lists;
using FadingFlame.Players;
using NUnit.Framework;

namespace FadingFlameTests
{
    public class PlayerTests : IntegrationTestBase
    {
        [Test]
        public async Task LoadRanked()
        {
            var player1 = Player.Create("dude", "lol@rofl.de");
            player1.AddWin(Mmr.Create());
            player1.AddLoss(Mmr.Create());
            player1.AddWin(Mmr.Create());
            
            var player2 = Player.Create("dude", "lol@rofl.de");
            player2.AddWin(Mmr.Create());
            player2.AddWin(Mmr.Create());

            var playerRepository = new PlayerRepository(MongoClient, new ListRepository(MongoClient));
            await playerRepository.Insert(player1);
            await playerRepository.Insert(player2);

            var rankedPlayers = await playerRepository.LoadAllRanked();
            
            var rankedPlayer = rankedPlayers.Single();
            Assert.AreEqual(2, rankedPlayer.Wins);
            Assert.AreEqual(3, rankedPlayer.MatchCount);
            Assert.AreEqual(1, rankedPlayer.Losses);
        }
    }
}