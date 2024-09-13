using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FadingFlame.Leagues;
using FadingFlame.Lists;
using FadingFlame.Matchups;
using FadingFlame.Players;
using FluentAssertions;
using MongoDB.Bson;
using NUnit.Framework;

namespace FadingFlameTests
{
    [TestFixture]
    public class LeagueTests : IntegrationTestBase
    {
        [Test]
        public async Task VersionSaveUpdate()
        {
            var leagueRepository = new LeagueRepository(MongoClient, new MatchupRepository(MongoClient));

            var league = League.Create(1, DateTime.Now, "1A", "Sunna");
            await leagueRepository.Insert(new List<League> { league });

            var leagueLoaded1 = await leagueRepository.Load(league.Id);
            var leagueLoaded2 = await leagueRepository.Load(league.Id);

            leagueLoaded1.AddPlayer(Player.Create("test", "test@lol.de"));
            leagueLoaded2.AddPlayer(Player.Create("test2", "test@lol.de"));

            var update1 = await leagueRepository.Update(leagueLoaded1);
            var update2 = await leagueRepository.Update(leagueLoaded2);

            Assert.IsTrue(update1);
            Assert.IsFalse(update2);

            var leagueLoadedAgain = await leagueRepository.Load(league.Id);

            Assert.AreEqual(1, leagueLoadedAgain.Players.Count);
            Assert.AreEqual(1, leagueLoadedAgain.Version);
        }

        [Test]
        public void AddPlayer()
        {
            var league = new League();

            var player = Player.Create("peter", "213");
            league.AddPlayer(player);

            league.Players.Should().Contain(p => p.Id == player.Id);
        }
        
        [TestCase(1, 0, 10, 10)]
        [TestCase(200, 0, 10, 10)]
        [TestCase(199, 0, 10, 10)]
        public void ShouldCalculateResult_Draw(
            int player1Points,
            int player2Points,
            int exectedoints1,
            int expectedPoints2)
        {
            var league = new League();

            var player1 = Player.Create("peter", "213");
            player1.Id = ObjectId.GenerateNewId();
            var player2 = Player.Create("wolf", "213");
            player2.Id = ObjectId.GenerateNewId();

            league.AddPlayer(player1);
            league.AddPlayer(player2);
            league.CreateGameDays();

            var result = new MatchResultDto
            {
                MatchId = league.GameDays.First().Matchups.First().Id,
                SecondaryObjective = SecondaryObjectiveState.draw,
                Player1 = new PlayerResultDto
                {
                    Id = player1.Id,
                    VictoryPoints = player1Points
                },
                Player2 = new PlayerResultDto
                {
                    Id = player2.Id,
                    VictoryPoints = player2Points
                }
            };

            league.ReportGame(TestUtils.MmrRepositoryMock(), result, player1.Mmr, player2.Mmr, null, null);

            league.Players[1].Id.Should().Be(player1.Id);
            league.Players[0].Id.Should().Be(player2.Id);

            league.Players[1].BattlePoints.Should().Be(exectedoints1);
            league.Players[0].BattlePoints.Should().Be(expectedPoints2);
        }

        [TestCase(201, 0, 11, 9)]
        [TestCase(399, 0, 11, 9)]
        [TestCase(400, 0, 11, 9)]
        [TestCase(401, 0, 12, 8)]
        [TestCase(799, 0, 12, 8)]
        [TestCase(800, 0, 12, 8)]
        [TestCase(801, 0, 13, 7)]
        [TestCase(1199, 0, 13, 7)]
        [TestCase(1200, 0, 13, 7)]
        [TestCase(1201, 0, 14, 6)]
        [TestCase(1599, 0, 14, 6)]
        [TestCase(1600, 0, 14, 6)]
        [TestCase(1601, 0, 15, 5)]
        [TestCase(1999, 0, 15, 5)]
        [TestCase(2000, 0, 15, 5)]
        [TestCase(2001, 0, 16, 4)]
        [TestCase(3150, 0, 16, 4)]
        public async Task ShouldCalculateResult_DrawInSecondObjectiveOneWins(
            int player1Points,
            int player2Points,
            int exectedoints1,
            int expectedPoints2)
        {
            var league = new League();

            var player1 = Player.Create("peter", "213");
            player1.Id = ObjectId.GenerateNewId();
            var player2 = Player.Create("wolf", "213");
            player2.Id = ObjectId.GenerateNewId();

            league.AddPlayer(player1);
            league.AddPlayer(player2);
            league.CreateGameDays();

            var result = new MatchResultDto
            {
                MatchId = league.GameDays.First().Matchups.First().Id,
                SecondaryObjective = SecondaryObjectiveState.draw,
                Player1 = new PlayerResultDto
                {
                    Id = player1.Id,
                    VictoryPoints = player1Points
                },
                Player2 = new PlayerResultDto
                {
                    Id = player2.Id,
                    VictoryPoints = player2Points
                }
            };

            await league.ReportGame(TestUtils.MmrRepositoryMock(), result, player1.Mmr, player2.Mmr, null, null);

            league.Players[0].Id.Should().Be(player1.Id);
            league.Players[1].Id.Should().Be(player2.Id);

            league.Players[0].BattlePoints.Should().Be(exectedoints1);
            league.Players[1].BattlePoints.Should().Be(expectedPoints2);
        }


        [TestCase(0, 201, 9, 11)]
        [TestCase(0, 399, 9, 11)]
        [TestCase(0, 400, 9, 11)]
        [TestCase(0, 401, 8, 12)]
        [TestCase(0, 799, 8, 12)]
        [TestCase(0, 800, 8, 12)]
        [TestCase(0, 801, 7, 13)]
        [TestCase(0, 1199, 7, 13)]
        [TestCase(0, 1200, 7, 13)]
        [TestCase(0, 1201, 6, 14)]
        [TestCase(0, 1599, 6, 14)]
        [TestCase(0, 1600, 6, 14)]
        [TestCase(0, 1601, 5, 15)]
        [TestCase(0, 1999, 5, 15)]
        [TestCase(0, 2000, 5, 15)]
        [TestCase(0, 2001, 4, 16)]
        [TestCase(0, 3150, 4, 16)]
        public void ShouldCalculateResult_DrawInSecondObjectiveTwoWins(
            int player1Points,
            int player2Points,
            int exectedoints1,
            int expectedPoints2)
        {
            var league = new League();

            var player1 = Player.Create("peter", "213");
            player1.Id = ObjectId.GenerateNewId();
            var player2 = Player.Create("wolf", "213");
            player2.Id = ObjectId.GenerateNewId();

            league.AddPlayer(player1);
            league.AddPlayer(player2);
            league.CreateGameDays();

            var result = new MatchResultDto
            {
                MatchId = league.GameDays.First().Matchups.First().Id,
                SecondaryObjective = SecondaryObjectiveState.draw,
                Player1 = new PlayerResultDto
                {
                    Id = player1.Id,
                    VictoryPoints = player1Points
                },
                Player2 = new PlayerResultDto
                {
                    Id = player2.Id,
                    VictoryPoints = player2Points
                }
            };

            league.ReportGame(TestUtils.MmrRepositoryMock(), result, player1.Mmr, player2.Mmr, null, null);

            league.Players[1].Id.Should().Be(player1.Id);
            league.Players[0].Id.Should().Be(player2.Id);

            league.Players[1].BattlePoints.Should().Be(exectedoints1);
            league.Players[0].BattlePoints.Should().Be(expectedPoints2);
        }

        [TestCase(1000, 1000, 13, 7)]
        [TestCase(1200, 1000, 13, 7)]
        [TestCase(1201, 1000, 14, 6)]
        [TestCase(1251, 1251, 13, 7)]
        [TestCase(2501, 2301, 13, 7)]
        [TestCase(2501, 2300, 14, 6)]
        [TestCase(451, 0, 15, 5)]
        [TestCase(901, 0, 16, 4)]
        [TestCase(1351, 0, 17, 3)]
        [TestCase(1801, 0, 18, 2)]
        [TestCase(2251, 0, 19, 1)]
        [TestCase(2252, 0, 19, 1)]
        [TestCase(2000, 0, 18, 2)]
        [TestCase(2001, 0, 19, 1)]
        public void ShouldCalculateResult_player1wins(
            int player1Points,
            int player2Points,
            int exectedoints1,
            int expectedPoints2)
        {
            var league = new League();

            var player1 = Player.Create("peter", "213");
            player1.Id = ObjectId.GenerateNewId();
            var player2 = Player.Create("wolf", "213");
            player2.Id = ObjectId.GenerateNewId();

            league.AddPlayer(player1);
            league.AddPlayer(player2);
            league.CreateGameDays();

            var result = new MatchResultDto
            {
                MatchId = league.GameDays.First().Matchups.First().Id,
                SecondaryObjective = SecondaryObjectiveState.player1,
                Player1 = new PlayerResultDto
                {
                    Id = player1.Id,
                    VictoryPoints = player1Points
                },
                Player2 = new PlayerResultDto
                {
                    Id = player2.Id,
                    VictoryPoints = player2Points
                }
            };

            league.ReportGame(TestUtils.MmrRepositoryMock(), result, player1.Mmr, player2.Mmr, null, null);

            league.Players[0].Id.Should().Be(player1.Id);
            league.Players[1].Id.Should().Be(player2.Id);

            league.Players[0].BattlePoints.Should().Be(exectedoints1);
            league.Players[1].BattlePoints.Should().Be(expectedPoints2);
        }

        [TestCase(1000, 1000, 13, 7)]
        [TestCase(1200, 1000, 13, 7)]
        [TestCase(1201, 1000, 14, 6)]
        [TestCase(1251, 1251, 13, 7)]
        [TestCase(2501, 2301, 13, 7)]
        [TestCase(2501, 2300, 14, 6)]
        [TestCase(451, 0, 15, 5)]
        [TestCase(901, 0, 16, 4)]
        [TestCase(1351, 0, 17, 3)]
        [TestCase(1801, 0, 18, 2)]
        [TestCase(2251, 0, 19, 1)]
        [TestCase(2252, 0, 19, 1)]
        [TestCase(2000, 0, 18, 2)]
        [TestCase(2001, 0, 19, 1)]
        public void ShouldCalculateResult_player2wins(
            int player2Points,
            int player1Points,
            int expectedPoints2,
            int exectedoints1)
        {
            var league = new League();

            var player1 = Player.Create("peter", "213");
            player1.Id = ObjectId.GenerateNewId();
            var player2 = Player.Create("wolf", "213");
            player2.Id = ObjectId.GenerateNewId();

            league.AddPlayer(player2);
            league.AddPlayer(player1);
            league.CreateGameDays();

            var result = new MatchResultDto
            {
                MatchId = league.GameDays.First().Matchups.First().Id,
                SecondaryObjective = SecondaryObjectiveState.player2,
                Player1 = new PlayerResultDto
                {
                    Id = player1.Id,
                    VictoryPoints = player1Points
                },
                Player2 = new PlayerResultDto
                {
                    Id = player2.Id,
                    VictoryPoints = player2Points
                }
            };

            league.ReportGame(TestUtils.MmrRepositoryMock(), result, player1.Mmr, player2.Mmr, null, null);

            league.Players[0].Id.Should().Be(player2.Id);
            league.Players[1].Id.Should().Be(player1.Id);

            league.Players[0].BattlePoints.Should().Be(expectedPoints2);
            league.Players[1].BattlePoints.Should().Be(exectedoints1);
        }
        
        [TestCase(SecondaryObjectiveState.draw, SecondaryObjectiveState.draw, 10, 10)]
        [TestCase(SecondaryObjectiveState.player1, SecondaryObjectiveState.draw, 13, 7)]
        [TestCase(SecondaryObjectiveState.draw, SecondaryObjectiveState.player1, 11, 9)]
        [TestCase(SecondaryObjectiveState.player2, SecondaryObjectiveState.draw, 7, 13)]
        [TestCase(SecondaryObjectiveState.draw, SecondaryObjectiveState.player2, 9, 11)]
        [TestCase(SecondaryObjectiveState.player1, SecondaryObjectiveState.player1, 14, 6)]
        [TestCase(SecondaryObjectiveState.player1, SecondaryObjectiveState.player2, 12, 8)]
        [TestCase(SecondaryObjectiveState.player2, SecondaryObjectiveState.player2, 6, 14)]
        [TestCase(SecondaryObjectiveState.player1, SecondaryObjectiveState.player2, 12, 8)]
        [TestCase(SecondaryObjectiveState.player2, SecondaryObjectiveState.player1, 8, 12)]
        public async Task SecondariesAndPrimaries(
            SecondaryObjectiveState primaryWinner,
            SecondaryObjectiveState secondaryWinner,
            int expectedPointsPlayer1,
            int expectedPointsPlayer2)
        {
            var league = new League();

            var player1 = Player.Create("peter", "213");
            player1.Id = ObjectId.GenerateNewId();
            var player2 = Player.Create("wolf", "213");
            player2.Id = ObjectId.GenerateNewId();

            league.AddPlayer(player2);
            league.AddPlayer(player1);
            league.CreateGameDays();

            var result = new MatchResultDto
            {
                MatchId = league.GameDays.First().Matchups.First().Id,
                SecondaryObjective = primaryWinner,
                _3_0_SecondaryObjective = secondaryWinner,
                Player1 = new PlayerResultDto
                {
                    Id = player1.Id,
                    VictoryPoints = 1000
                },
                Player2 = new PlayerResultDto
                {
                    Id = player2.Id,
                    VictoryPoints = 1000
                }
            };

            var matchResult = await league.ReportGame(TestUtils.MmrRepositoryMock(), result, player1.Mmr, player2.Mmr, null, null);

            matchResult.Player1.BattlePoints.Should().Be(expectedPointsPlayer1);
            matchResult.Player2.BattlePoints.Should().Be(expectedPointsPlayer2);
        }

        [Test]
        public void DeleteGameReport()
        {
            var league = new League();

            var player1 = Player.Create("peter", "213");
            player1.Id = ObjectId.GenerateNewId();
            var player2 = Player.Create("wolf", "213");
            player2.Id = ObjectId.GenerateNewId();

            league.AddPlayer(player2);
            league.AddPlayer(player1);
            league.CreateGameDays();

            var result = new MatchResultDto
            {
                MatchId = league.GameDays.First().Matchups.First().Id,
                SecondaryObjective = SecondaryObjectiveState.player1,
                Player1 = new PlayerResultDto
                {
                    Id = player1.Id,
                    VictoryPoints = 500
                },
                Player2 = new PlayerResultDto
                {
                    Id = player2.Id,
                    VictoryPoints = 100
                }
            };

            league.ReportGame(TestUtils.MmrRepositoryMock(), result, player1.Mmr, player2.Mmr, null, null);

            league.Players[0].Id.Should().Be(player1.Id);
            league.Players[1].Id.Should().Be(player2.Id);

            league.Players[0].VictoryPoints.Should().Be(500);
            league.Players[1].VictoryPoints.Should().Be(100);
            
            league.DeleteGameReport(result.MatchId);
            
            league.Players[0].VictoryPoints.Should().Be(0);
            league.Players[1].VictoryPoints.Should().Be(0);
            league.Players[0].BattlePoints.Should().Be(0);
            league.Players[1].BattlePoints.Should().Be(0);
            league.Players[0].GamesCount.Should().Be(0);
            league.Players[1].GamesCount.Should().Be(0);
        }

        [Test]
        public void ListComparerBattlePoints()
        {
            var league = CreateLeagueToOrder();

            league.Players[1].BattlePoints = 7;

            league.ReorderPlayers();

            Assert.AreEqual(7, league.Players[0].BattlePoints);
        }

        [Test]
        public void PenaltyPoints_Subtracting()
        {
            var league = CreateLeagueToOrder();
            league.PenaltyPointsForPlayer(league.Players[0].Id, -10);
            league.PenaltyPointsForPlayer(league.Players[3].Id, -10);

            Assert.AreEqual(-10, league.Players[3].PenaltyPoints);
            Assert.AreEqual(-10, league.Players[3].BattlePoints);
        }

        [Test]
        public void PenaltyPoints_Adding()
        {
            var league = CreateLeagueToOrder();
            league.PenaltyPointsForPlayer(league.Players[0].Id, 10);
            league.PenaltyPointsForPlayer(league.Players[0].Id, 10);

            Assert.AreEqual(10, league.Players[0].PenaltyPoints);
            Assert.AreEqual(10, league.Players[0].BattlePoints);
        }

        [Test]
        public void PenaltyPoints_AddingDifferent()
        {
            var league = CreateLeagueToOrder();
            league.PenaltyPointsForPlayer(league.Players[0].Id, 10);
            league.PenaltyPointsForPlayer(league.Players[0].Id, 5);

            Assert.AreEqual(5, league.Players[0].PenaltyPoints);
            Assert.AreEqual(5, league.Players[0].BattlePoints);
        }

        [Test]
        public void PenaltyPoints_AddingDifferent_WithStartingPoints()
        {
            var league = CreateLeagueToOrder();
            league.Players[0].BattlePoints = 40;
            league.PenaltyPointsForPlayer(league.Players[0].Id, 10);
            league.PenaltyPointsForPlayer(league.Players[0].Id, 5);

            Assert.AreEqual(5, league.Players[0].PenaltyPoints);
            Assert.AreEqual(45, league.Players[0].BattlePoints);
        }

        [Test]
        public void ListComparerVictoryPoints()
        {
            var league = CreateLeagueToOrder();

            league.Players[1].VictoryPoints = 7;

            league.ReorderPlayers();

            Assert.AreEqual(7, league.Players[0].VictoryPoints);
        }

        [Test]
        public async Task ListComparerDirectComComparison()
        {
            var league = CreateLeagueToOrder();

            league.Players[0].BattlePoints = 20;
            league.Players[1].BattlePoints = 0;

            var id1 = league.Players[0].Id;
            var id2 = league.Players[1].Id;
            var result = new MatchResultDto
            {
                MatchId = league.GameDays[2].Matchups[0].Id,
                SecondaryObjective = SecondaryObjectiveState.player2,
                _3_0_SecondaryObjective = SecondaryObjectiveState.player2,
                Player1 = new PlayerResultDto
                {
                    Id = id1,
                    VictoryPoints = 0
                },
                Player2 = new PlayerResultDto
                {
                    Id = id2,
                    VictoryPoints = 4000
                }
            };

            await league.ReportGame(TestUtils.MmrRepositoryMock(), result, Mmr.Create(), Mmr.Create(), null, null);

            Assert.AreEqual(20, league.Players[0].BattlePoints);
            Assert.AreEqual(20, league.Players[1].BattlePoints);
            Assert.AreEqual(id2, league.Players[0].Id);
            Assert.AreEqual(id1, league.Players[1].Id);
        }
        
        private static League CreateLeagueToOrder()
        {
            var league = new League();

            var player1 = Player.Create("peter", "1");
            player1.Id = ObjectId.GenerateNewId();
            var player2 = Player.Create("wolf", "2");
            player2.Id = ObjectId.GenerateNewId();
            var player3 = Player.Create("dude", "3");
            player3.Id = ObjectId.GenerateNewId();
            var player4 = Player.Create("test", "4");
            player4.Id = ObjectId.GenerateNewId();

            league.AddPlayer(player4);
            league.AddPlayer(player3);
            league.AddPlayer(player2);
            league.AddPlayer(player1);
            league.CreateGameDays();
            return league;
        }

        [Test]
        public void AssertMethodTest_HappyPath()
        {
            var match = CreateDefaultMatchup();
            var match2 = CreateDefaultMatchup();
            var gameDays = new List<GameDay>
            {
                CreateGameDay(match, match2)
            };

            AssertMatchIsNeverPlayedTwice(gameDays);
            Assert.IsTrue(AssertMatchIsNeverPlayedTwice(gameDays));
        }

        [Test]
        public async Task LoadLeaguesOflayer()
        {
            var league1 = League.Create(1, DateTime.Now, "1a", "123");
            var league2 = League.Create(1, DateTime.Now, "2a", "123");
            var league3 = League.Create(2, DateTime.Now, "1a", "123");

            var player = Player.Create("dude", "mail");
            var playerRepository = new PlayerRepository(MongoClient, new ListRepository(MongoClient));
            await playerRepository.Insert(player);

            league2.AddPlayer(player);
            league3.AddPlayer(player);

            var leagueRepository = new LeagueRepository(MongoClient, new MatchupRepository(MongoClient));

            await leagueRepository.Insert(new List<League> { league1, league2, league3 });

            var leaguesForPlayer = await leagueRepository.LoadLeaguesForPlayer(player.Id);

            Assert.AreEqual(2, leaguesForPlayer.Count);

            Assert.AreEqual(1, leaguesForPlayer[0].Season);
            Assert.AreEqual("2a", leaguesForPlayer[0].DivisionId);
            Assert.AreEqual(2, leaguesForPlayer[1].Season);
            Assert.AreEqual("1a", leaguesForPlayer[1].DivisionId);
        }

        [Test]
        public void AssertMethodTest_DoubleGame()
        {
            var match = CreateDefaultMatchup();
            var gameDays = new List<GameDay>
            {
                CreateGameDay(match, match)
            };

            AssertMatchIsNeverPlayedTwice(gameDays);
            Assert.IsFalse(AssertMatchIsNeverPlayedTwice(gameDays));
        }

        [Test]
        public void AssertMethodTest_switched()
        {
            var player1 = ObjectId.GenerateNewId();
            var player2 = ObjectId.GenerateNewId();
            var match = CreateDefaultMatchup(player1, player2);
            var matchSwitched = CreateDefaultMatchup(player1, player2);


            var gameDays = new List<GameDay>
            {
                CreateGameDay(match, matchSwitched)
            };

            Assert.IsFalse(AssertMatchIsNeverPlayedTwice(gameDays));
        }

        private static GameDay CreateGameDay(Matchup match, Matchup matchSwitched)
        {
            var matchups = new List<Matchup>
            {
                match, matchSwitched
            };

            var gameDay = GameDay.Create(DateTime.Now, matchups);
            return gameDay;
        }

        [Test]
        public void MakePairings_GameDaysOk()
        {
            var league = TestUtils.CreateLeagueWithPlayers(ObjectId.GenerateNewId(), ObjectId.GenerateNewId(), ObjectId.GenerateNewId(), ObjectId.GenerateNewId());

            league.CreateGameDays();

            Assert.AreEqual(3, league.GameDays.Count);
        }

        [Test]
        public void MakePairings_PairingsOk_TwoPlayers()
        {
            var player1 = ObjectId.GenerateNewId();
            var player2 = ObjectId.GenerateNewId();
            var league = TestUtils.CreateLeagueWithPlayers(player1, player2);

            league.CreateGameDays();

            Assert.AreEqual(1, league.GameDays.Count);
            Assert.IsTrue(AssertMatchIsNeverPlayedTwice(league.GameDays));
        }

        [Test]
        public void MakePairings_PairingsOk_fourPlayers()
        {
            var player1 = ObjectId.GenerateNewId();
            var player2 = ObjectId.GenerateNewId();
            var player3 = ObjectId.GenerateNewId();
            var player4 = ObjectId.GenerateNewId();
            var league = TestUtils.CreateLeagueWithPlayers(player1, player2, player3, player4);

            league.CreateGameDays();

            var domainEventGameDays = league.GameDays.ToList();
            Assert.AreEqual(3, domainEventGameDays.Count);
            Assert.IsTrue(AssertMatchIsNeverPlayedTwice(domainEventGameDays));
        }

        [Test]
        public void MakePairings_PairingsOk_fivePlayers()
        {
            var player1 = ObjectId.GenerateNewId();
            var player2 = ObjectId.GenerateNewId();
            var player3 = ObjectId.GenerateNewId();
            var player4 = ObjectId.GenerateNewId();
            var player5 = ObjectId.GenerateNewId();
            var league = TestUtils.CreateLeagueWithPlayers(player1, player2, player3, player4, player5);

            league.CreateGameDays();

            var domainEventGameDays = league.GameDays.ToList();
            Assert.AreEqual(5, domainEventGameDays.Count);
            Assert.AreEqual(5, league.Players.Count);
            Assert.IsTrue(AssertMatchIsNeverPlayedTwice(domainEventGameDays));
        }

        [Test]
        public void MakePairings_PairingsOk_sixPlayers()
        {
            var player1 = ObjectId.GenerateNewId();
            var player2 = ObjectId.GenerateNewId();
            var player3 = ObjectId.GenerateNewId();
            var player4 = ObjectId.GenerateNewId();
            var player5 = ObjectId.GenerateNewId();
            var player6 = ObjectId.GenerateNewId();
            var league = TestUtils.CreateLeagueWithPlayers(player1, player2, player3, player4, player5, player6);

            league.CreateGameDays();

            var domainEventGameDays = league.GameDays.ToList();
            Assert.AreEqual(5, domainEventGameDays.Count);
            Assert.IsTrue(AssertMatchIsNeverPlayedTwice(domainEventGameDays));
        }

        [Test]
        public void MakePairings_PairingsOk_eightPlayers()
        {
            var player1 = ObjectId.GenerateNewId();
            var player2 = ObjectId.GenerateNewId();
            var player3 = ObjectId.GenerateNewId();
            var player4 = ObjectId.GenerateNewId();
            var player5 = ObjectId.GenerateNewId();
            var player6 = ObjectId.GenerateNewId();
            var player7 = ObjectId.GenerateNewId();
            var player8 = ObjectId.GenerateNewId();
            var league = TestUtils.CreateLeagueWithPlayers(player1, player2, player3, player4, player5, player6, player7, player8);

            league.CreateGameDays();

            var domainEventGameDays = league.GameDays.ToList();
            Assert.AreEqual(5, domainEventGameDays.Count);
            Assert.IsTrue(AssertMatchIsNeverPlayedTwice(domainEventGameDays));
        }

        private static Matchup CreateDefaultMatchup(ObjectId? player1 = null, ObjectId? player2 = null)
        {
            var playerInLeague1 = PlayerInLeague.Create(player1 ?? ObjectId.GenerateNewId());
            var playerInLeague2 = PlayerInLeague.Create(player2 ?? ObjectId.GenerateNewId());
            var matchup = Matchup.CreateForLeague(playerInLeague1, playerInLeague2);
            return matchup;
        }

        private bool AssertMatchIsNeverPlayedTwice(IEnumerable<GameDay> gameDays)
        {
            var allMatches = gameDays.SelectMany(g => g.Matchups).ToList();
            foreach (var match in allMatches)
            {
                var matchWith = allMatches.Where(m => m.Player1 == match.Player1 && m.Player2 == match.Player2
                                                      || m.Player1 == match.Player2 && m.Player2 == match.Player1);
                if (matchWith.Count() != 1) return false;
            }

            return true;
        }
    }
}