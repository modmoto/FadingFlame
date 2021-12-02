using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FadingFlame;
using FadingFlame.GlobalLadder;
using FadingFlame.Lists;
using FadingFlame.Matchups;
using FadingFlame.Players;
using FadingFlame.ReadModelBase;
using MongoDB.Bson;
using Moq;
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

        [Test]
        public async Task TestReadModelHandler()
        {
            var player1 = Player.Create("dude", "lol@rofl.de");
            var player2 = Player.Create("dude", "lol@rofl.de");

            var playerRepository = new PlayerRepository(MongoClient, new ListRepository(MongoClient));
            await playerRepository.Insert(player1);
            await playerRepository.Insert(player2);
            var challengeGame = Matchup.CreateChallengeGame(player1.Id, player2.Id);
            var matchupRepository = new MatchupRepository(MongoClient);
            await matchupRepository.InsertMatch(challengeGame);

            var player1ResultDto = new PlayerResultDto
            {
                VictoryPoints = 1500,
                Id = player1.Id
            };
            var player2ResultDto = new PlayerResultDto
            {
                VictoryPoints = 500,
                Id = player2.Id
            };
            var player1List = GameList.Create("test1", "asd", Faction.Makhar);
            var player2List = GameList.Create("test2", "asd", Faction.Hobgoblins);
            var mock = new Mock<IMmrRepository>();
            mock.Setup(m => m.UpdateMmrs(It.IsAny<UpdateMmrRequest>())).ReturnsAsync(new List<Mmr>
            {
                new()
                {
                    Rating = 1600
                },
                new()
                {
                    Rating = 1400
                }
            });
            var matchResult = await MatchResult.Create(TestUtils.MmrRepositoryMock(), SecondaryObjectiveState.player1, player1.Mmr, player2.Mmr, player1ResultDto, player2ResultDto, player1List, player2List, false);
            challengeGame.RecordResult(matchResult);
            await matchupRepository.UpdateMatch(challengeGame);
            await playerRepository.Update(player1);
            await playerRepository.Update(player2);

            var rankingReadmodelRepository = new RankingReadmodelRepository(MongoClient);
            var playerRankingModelReadHandler = new PlayerRankingModelReadHandler(matchupRepository, playerRepository, rankingReadmodelRepository);
            await playerRankingModelReadHandler.Update(new HandlerVersion());

            var ranked = await rankingReadmodelRepository.LoadAllRanked();

            Assert.AreEqual(2, ranked.Count);
            Assert.AreEqual((int) player1.Mmr.Rating, ranked[0].Mmr);
            Assert.AreEqual((int) player2.Mmr.Rating, ranked[1].Mmr);
        }
    }
}