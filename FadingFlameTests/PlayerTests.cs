using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FadingFlame.GlobalLadder;
using FadingFlame.Matchups;
using FadingFlame.Players;
using MongoDB.Bson;
using NUnit.Framework;

namespace FadingFlameTests
{
    public class PlayerTests : IntegrationTestBase
    {
        [Test]
        public async Task LoadRanked()
        {
            var player = Player.Create("dude", "lol@rofl.de");
            player.Id = ObjectId.GenerateNewId();
            var playerReadModel = PlayerRankingReadModel.Create(player, new List<MatchResult>
            {
                new ()
                {
                    Winner = player.Id
                },
                new ()
                {
                    Winner = ObjectId.Empty
                },
                new ()
                {
                    Winner = ObjectId.GenerateNewId()
                },
                new ()
                {
                    Winner = player.Id
                }
            });

            var playerRepository = new RankingReadmodelRepository(MongoClient);
            await playerRepository.Upsert(new List<PlayerRankingReadModel> { playerReadModel } );

            var rankedPlayers = await playerRepository.LoadAllRanked();
            
            var rankedPlayer = rankedPlayers.Single();
            Assert.AreEqual(2, rankedPlayer.Wins);
            Assert.AreEqual(4, rankedPlayer.MatchCount);
            Assert.AreEqual(1, rankedPlayer.Losses);
            Assert.AreEqual(1, rankedPlayer.Draws);
        }
    }
}