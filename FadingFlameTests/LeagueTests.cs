using System;
using System.Collections.Generic;
using System.Linq;
using FadingFlame.Leagues;
using FadingFlame.Players;
using FluentAssertions;
using MongoDB.Bson;
using NUnit.Framework;

namespace FadingFlameTests
{
    [TestFixture]
    public class LeagueTests
    {
        [Test]
        public void AddPlayer()
        {
            var league = new League();

            var player = Player.Create("peter", "213");
            league.AddPlayer(player);

            league.Players.Should().Contain(p => p.Id == player.Id);
        }

        [TestCase(1000, 1000, 13, 7)]
        [TestCase(1225, 1000, 13, 7)]
        [TestCase(1226, 1000, 14, 6)]
        [TestCase(1251, 1251, 13, 7)]
        [TestCase(2501, 2276, 13, 7)]
        [TestCase(2501, 2275, 14, 6)]
        [TestCase(451, 0, 15, 5)]
        [TestCase(901, 0, 16, 4)]
        [TestCase(1351, 0, 17, 3)]
        [TestCase(1801, 0, 18, 2)]
        [TestCase(2251, 0, 19, 1)]
        [TestCase(2252, 0, 19, 1)]
        [TestCase(3150, 0, 19, 1)]
        [TestCase(3151, 0, 20, 0)]
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
                MatchId = league.GameDays.First().Matchups.First().MatchId,
                SecondaryObjective = SecondaryObjectiveState.player1,
                Player1 = new PlayerResultDto
                {
                    Id = player1.Id,
                    VictoryPoints = player1Points,
                },
                Player2 = new PlayerResultDto
                {
                    Id = player2.Id,
                    VictoryPoints = player2Points,
                }
            };

            league.ReportGame(result);

            league.Players[0].Id.Should().Be(player1.Id);
            league.Players[1].Id.Should().Be(player2.Id);

            league.Players[0].Points.Should().Be(exectedoints1);
            league.Players[1].Points.Should().Be(expectedPoints2);
        }

        [TestCase(1000, 1000, 13, 7)]
        [TestCase(1225, 1000, 13, 7)]
        [TestCase(1226, 1000, 14, 6)]
        [TestCase(1251, 1251, 13, 7)]
        [TestCase(2501, 2276, 13, 7)]
        [TestCase(2501, 2275, 14, 6)]
        [TestCase(451, 0, 15, 5)]
        [TestCase(901, 0, 16, 4)]
        [TestCase(1351, 0, 17, 3)]
        [TestCase(1801, 0, 18, 2)]
        [TestCase(2251, 0, 19, 1)]
        [TestCase(2252, 0, 19, 1)]
        [TestCase(3150, 0, 19, 1)]
        [TestCase(3151, 0, 20, 0)]
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
                MatchId = league.GameDays.First().Matchups.First().MatchId,
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

            league.ReportGame(result);

            league.Players[0].Id.Should().Be(player2.Id);
            league.Players[1].Id.Should().Be(player1.Id);

            league.Players[0].Points.Should().Be(expectedPoints2);
            league.Players[1].Points.Should().Be(exectedoints1);
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

            var gameDay = GameDay.Create(DateTimeOffset.Now, matchups);
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
            var league = TestUtils.CreateLeagueWithPlayers(player1, player2, player3, player4, player5, player6, player7,
            player8);

            league.CreateGameDays();

            var domainEventGameDays = league.GameDays.ToList();
            Assert.AreEqual(5, domainEventGameDays.Count);
            Assert.IsTrue(AssertMatchIsNeverPlayedTwice(domainEventGameDays));
        }


        private static Matchup CreateDefaultMatchup(ObjectId? player1 = null, ObjectId? player2 = null)
        {
            var playerInLeague1 = PlayerInLeague.Create(player1 ?? ObjectId.GenerateNewId());
            var playerInLeague2 = PlayerInLeague.Create(player2 ?? ObjectId.GenerateNewId());
            var matchup = Matchup.Create(playerInLeague1, playerInLeague2);
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