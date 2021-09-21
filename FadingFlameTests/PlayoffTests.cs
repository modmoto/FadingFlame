using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FadingFlame.Admin;
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
                new PlayoffRepository(MongoClient), new SeasonState());

            var playoffs = await playoffCommandHandler.CreatePlayoffs();

            Assert.AreEqual(3, playoffs.Rounds.Count);
            Assert.AreEqual(4, playoffs.Rounds.First().Matchups.Count);
        }

        [Test]
        public async Task AdvanceRoundsAutomatically()
        {
            var leagueRepository = new Mock<ILeagueRepository>();
            leagueRepository.Setup(l => l.LoadForSeason(It.IsAny<int>())).
                ReturnsAsync(TestLeagues(4).ToList());

            var playoffCommandHandler = new PlayoffCommandHandler(
                leagueRepository.Object,
                new PlayoffRepository(MongoClient), new SeasonState());

            var playoffs = await playoffCommandHandler.CreatePlayoffs();

            var matchup1 = playoffs.Rounds[0].Matchups[0];
            var matchup2 = playoffs.Rounds[0].Matchups[1];
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

            Assert.AreEqual(2, playoffs.Rounds[0].Matchups.Count);
            Assert.AreEqual(1, playoffs.Rounds[1].Matchups.Count);

            var finale = playoffs.Rounds[1].Matchups[0];
            Assert.AreEqual(matchup1.Player2,  finale.Player1);
            Assert.AreEqual(matchup2.Player1,  finale.Player2);
        }

        [Test]
        public async Task PlayoffsTwoTooMany()
        {
            var leagueRepository = new Mock<ILeagueRepository>();
            leagueRepository.Setup(l => l.LoadForSeason(It.IsAny<int>())).
                ReturnsAsync(TestLeagues(6).ToList());

            var playoffCommandHandler = new PlayoffCommandHandler(
                leagueRepository.Object,
                new PlayoffRepository(MongoClient), new SeasonState());

            var playoffs = await playoffCommandHandler.CreatePlayoffs();
            
            Assert.AreEqual(3, playoffs.Rounds.Count);
            Assert.AreEqual(4, playoffs.Rounds[0].Matchups.Count);
            var playoffsRound = playoffs.Rounds[0];
            Assert.IsTrue(playoffsRound.Matchups[0].IsFinished);
            Assert.AreEqual(ObjectId.Empty, playoffsRound.Matchups[0].Player2);
            Assert.AreEqual(ObjectId.Empty, playoffsRound.Matchups[1].Player2);
            Assert.IsTrue(playoffsRound.Matchups[0].IsFinished);
            Assert.IsTrue(playoffsRound.Matchups[1].IsFinished);
            Assert.IsFalse(playoffsRound.Matchups[2].IsFinished);
            Assert.IsFalse(playoffsRound.Matchups[3].IsFinished);
        }
        
        [Test]
        public async Task PlayoffsTwoTooMany_BigTournament()
        {
            var leagueRepository = new Mock<ILeagueRepository>();
            leagueRepository.Setup(l => l.LoadForSeason(It.IsAny<int>())).
                ReturnsAsync(TestLeagues(18).ToList());

            var playoffCommandHandler = new PlayoffCommandHandler(
                leagueRepository.Object,
                new PlayoffRepository(MongoClient), new SeasonState());

            var playoffs = await playoffCommandHandler.CreatePlayoffs();
            
            Assert.AreEqual(5, playoffs.Rounds.Count);
            Assert.AreEqual(16, playoffs.Rounds[0].Matchups.Count);
            var playoffsRound = playoffs.Rounds[0];
            Assert.IsTrue(playoffsRound.Matchups[0].IsFinished);
            Assert.AreEqual(ObjectId.Empty, playoffsRound.Matchups[0].Player2);
            Assert.AreEqual(ObjectId.Empty, playoffsRound.Matchups[1].Player2);
            Assert.IsTrue(playoffsRound.Matchups[0].IsFinished);
            Assert.IsTrue(playoffsRound.Matchups[1].IsFinished);
            Assert.IsTrue(playoffsRound.Matchups[13].IsFinished);
            Assert.IsFalse(playoffsRound.Matchups[14].IsFinished);
            Assert.IsFalse(playoffsRound.Matchups[15].IsFinished);
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