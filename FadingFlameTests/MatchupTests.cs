using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FadingFlame.GlobalLadder;
using FadingFlame.Lists;
using FadingFlame.Matchups;
using FadingFlame.Players;
using MongoDB.Bson;
using NUnit.Framework;

namespace FadingFlameTests
{
    public class MatchupTests : IntegrationTestBase
    {
        [Test]
        public async Task LoadMatchupsWithName()
        {
            var matchupRepository = new MatchupRepository(MongoClient);
            var playerRepository = new PlayerRepository(MongoClient, new ListRepository(MongoClient));

            var player1 = Player.Create("test1", "rofl");
            player1.DiscordTag = "discor1";
            var player2 = Player.Create("test2", "rofl");
            player2.DiscordTag = "discor1";
            await playerRepository.Insert(player1);
            await playerRepository.Insert(player2);

            var challengeGame = Matchup.CreateChallengeGame(player1, player2);
            await matchupRepository.InsertMatch(challengeGame);

            var loadedMatches = await matchupRepository.LoadMatchesOfPlayer(player1);

            var matchup = loadedMatches.Single();
            Assert.AreEqual(player1.DisplayName, matchup.Player1Name);
            Assert.AreEqual(player2.DisplayName, matchup.Player2Name);
            Assert.AreEqual(player1.DiscordTag, matchup.Player1DiscordTag);
            Assert.AreEqual(player2.DiscordTag, matchup.Player2DiscordTag);
        }
    }
}