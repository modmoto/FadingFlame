using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FadingFlame.Players;
using FadingFlame.Repositories;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FadingFlame.Leagues
{
    public class League : IIdentifiable
    {
        public string Name { get; set; }
        public int Season { get; set; }
        [BsonId]
        public ObjectId Id { get; set; }
        public List<PlayerInLeague> Players { get; set; } = new();

        public void ReportGame(MatchResult matchResult)
        {
            var player1Result = matchResult.Player1;
            var player2Result = matchResult.Player2;

            var player1 = Players.FirstOrDefault(p => p.Id == player1Result.Id);
            var player2 = Players.FirstOrDefault(p => p.Id == player2Result.Id);

            if (player1 == null || player2 == null)
            {
                throw new ValidationException("Players are not in this league");
            }

            var points = CalculateWinPoints(player1Result.BattlePoints, player2Result.BattlePoints);

            var pointsAfteObjective = CalculateSecondaryObjective(
                player1Result,
                player2Result,
                points.Player1,
                points.Player2);

            player1.RecordResult(pointsAfteObjective.Player1);
            player2.RecordResult(pointsAfteObjective.Player2);

            player1.RecordResult(matchResult);
            player2.RecordResult(matchResult);

            Players = Players.OrderByDescending(p => p.Points).ToList();
        }

        private static PointTuple CalculateWinPoints(int player1, int player2)
        {
            var pointTuples = new Dictionary<int, PointTuple>
            {
                { 225, new PointTuple(10, 10)},
                { 450, new PointTuple(11, 9)},
                { 900, new PointTuple(12, 8)},
                { 1350, new PointTuple(13, 7)},
                { 1800, new PointTuple(14, 6)},
                { 2250, new PointTuple(15, 5)},
                { 3150, new PointTuple(16, 4)},
                { 3151, new PointTuple(17, 3)}
            };

            var pair = pointTuples.First(p => Math.Abs(player1 - player2) <= p.Key);

            return player1 > player2
                ? new PointTuple(pair.Value.Player1, pair.Value.Player2)
                : new PointTuple(pair.Value.Player2, pair.Value.Player1);
        }

        private static PointTuple CalculateSecondaryObjective(
            PlayerResult player1Result,
            PlayerResult player2Result,
            int points1,
            int points2)
        {
            if (player1Result.SecondaryObjective == SecondaryObjectiveState.won
                && player2Result.SecondaryObjective == SecondaryObjectiveState.lost)
            {
                return new PointTuple(points1 + 3, points2 - 3);
            }

            if (player1Result.SecondaryObjective == SecondaryObjectiveState.lost
                && player2Result.SecondaryObjective == SecondaryObjectiveState.won)
            {

                return new PointTuple(points1 - 3, points2 + 3);
            }

            if (player1Result.SecondaryObjective == SecondaryObjectiveState.draw
                && player2Result.SecondaryObjective == SecondaryObjectiveState.draw)
            {
                return new PointTuple(points1, points2);
            }

            throw new ValidationException("Invalid secondary objective selection");
        }

        public void AddPlayer(Player player)
        {
            Players = Players.Where(p => p.Id != player.Id).ToList();
            var playerInLeague = PlayerInLeague.Create(player.Id, player.DisplayName);
            Players.Add(playerInLeague);
        }

        public static League Create(int season)
        {
            return new()
            {
                Season = season
            };
        }
    }
}