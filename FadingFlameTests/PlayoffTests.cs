using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FadingFlame.Leagues;
using FadingFlame.Playoffs;
using MongoDB.Bson;
using Moq;
using NUnit.Framework;

namespace FadingFlameTests
{
    [TestFixture]
    public class PlayoffTests : IntegrationTestBase
    {
        [Test]
        public async Task CreateFirstPlayoffs()
        {
            var leagueRepository = new Mock<ILeagueRepository>();
            leagueRepository.Setup(l => l.LoadForSeason(It.IsAny<int>())).
                ReturnsAsync(TestLeagues(8).ToList());

            var playoffCommandHandler = new PlayoffCommandHandler(
                leagueRepository.Object,
                new PlayoffRepository(MongoClient));

            var playoffs = await playoffCommandHandler.CreatePlayoffs();

            Assert.AreEqual(1, playoffs.Rounds.Count);
            Assert.AreEqual(playoffs.Rounds.First().Matchups.Count, 4);
        }

        [Test]
        public async Task AdvanceRounds_NotFinished()
        {
            var leagueRepository = new Mock<ILeagueRepository>();
            leagueRepository.Setup(l => l.LoadForSeason(It.IsAny<int>())).
                ReturnsAsync(TestLeagues(8).ToList());

            var playoffCommandHandler = new PlayoffCommandHandler(
                leagueRepository.Object,
                new PlayoffRepository(MongoClient));

            var playoffs = await playoffCommandHandler.CreatePlayoffs();

            Assert.Throws<ValidationException>(() => playoffs.AdvanceToNextStage());
        }

        [Test]
        public async Task AdvanceRounds()
        {
            var leagueRepository = new Mock<ILeagueRepository>();
            leagueRepository.Setup(l => l.LoadForSeason(It.IsAny<int>())).
                ReturnsAsync(TestLeagues(4).ToList());

            var playoffCommandHandler = new PlayoffCommandHandler(
                leagueRepository.Object,
                new PlayoffRepository(MongoClient));

            var playoffs = await playoffCommandHandler.CreatePlayoffs();

            var matchup1 = playoffs.Rounds.Last().Matchups[0];
            var matchup2 = playoffs.Rounds.Last().Matchups[1];
            playoffs.ReportGame(new MatchResultDto
            {
                Player1 = new PlayerResultDto
                {
                    Id = matchup1.Player1,
                    VictoryPoints = 1000
                },
                Player2 = new PlayerResultDto
                {
                    Id = matchup1.Player2,
                    VictoryPoints = 1200
                },
                MatchId = matchup1.MatchId
            });
            playoffs.ReportGame(new MatchResultDto
            {
                Player1 = new PlayerResultDto
                {
                    Id = matchup2.Player1,
                    VictoryPoints = 1200
                },
                Player2 = new PlayerResultDto
                {
                    Id = matchup2.Player2,
                    VictoryPoints = 1000
                },
                MatchId = matchup2.MatchId
            });

            playoffs.AdvanceToNextStage();

            Assert.AreEqual(2, playoffs.Rounds[0].Matchups.Count);
            Assert.AreEqual(1, playoffs.Rounds[1].Matchups.Count);

            var finale = playoffs.Rounds[1].Matchups[0];
            Assert.AreEqual(matchup1.Player2,  finale.Player1);
            Assert.AreEqual(matchup2.Player1,  finale.Player2);
        }

        private IEnumerable<League> TestLeagues(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var leagueWithPlayers = TestUtils.CreateLeagueWithPlayers(
                    ObjectId.GenerateNewId(),
                    ObjectId.GenerateNewId(),
                    ObjectId.GenerateNewId(),
                    ObjectId.GenerateNewId());
                yield return leagueWithPlayers;
            }
        }
    }
}