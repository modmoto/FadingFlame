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

            var player = Player.Create("peter");
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

            var player1 = Player.Create("peter");
            player1.Id = ObjectId.GenerateNewId();
            var player2 = Player.Create("wolf");
            player2.Id = ObjectId.GenerateNewId();

            league.AddPlayer(player1);
            league.AddPlayer(player2);

            var result = new MatchResult
            {
                Player1 = new PlayerResult
                {
                    Id = player1.Id,
                    BattlePoints = player1Points,
                    SecondaryObjective = SecondaryObjectiveState.won
                },
                Player2 = new PlayerResult
                {
                    Id = player2.Id,
                    BattlePoints = player2Points,
                    SecondaryObjective = SecondaryObjectiveState.lost
                }
            };

            league.ReportGame(result);

            league.Players[0].Name.Should().Be(player1.DisplayName);
            league.Players[0].Id.Should().Be(player1.Id);
            league.Players[1].Name.Should().Be(player2.DisplayName);
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

            var player1 = Player.Create("peter");
            player1.Id = ObjectId.GenerateNewId();
            var player2 = Player.Create("wolf");
            player2.Id = ObjectId.GenerateNewId();

            league.AddPlayer(player2);
            league.AddPlayer(player1);

            var result = new MatchResult
            {
                Player1 = new PlayerResult
                {
                    Id = player1.Id,
                    BattlePoints = player1Points,
                    SecondaryObjective = SecondaryObjectiveState.lost
                },
                Player2 = new PlayerResult
                {
                    Id = player2.Id,
                    BattlePoints = player2Points,
                    SecondaryObjective = SecondaryObjectiveState.won
                }
            };

            league.ReportGame(result);

            league.Players[0].Name.Should().Be(player2.DisplayName);
            league.Players[0].Id.Should().Be(player2.Id);
            league.Players[1].Name.Should().Be(player1.DisplayName);
            league.Players[1].Id.Should().Be(player1.Id);

            league.Players[0].Points.Should().Be(expectedPoints2);
            league.Players[1].Points.Should().Be(exectedoints1);
        }
    }
}