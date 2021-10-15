using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FadingFlame.Leagues;
using FadingFlame.Lists;
using MongoDB.Bson;

namespace FadingFlame.Matchups
{
    public class MatchResult
    {
        public ObjectId MatchId  { get; set; }
        public PlayerResult Player1 { get; set; }
        public PlayerResult Player2 { get; set; }
        public SecondaryObjectiveState SecondaryObjective { get; set; }
        public int VictoryPointsDifference => Math.Abs((Player1?.VictoryPoints ?? 0) - (Player2?.VictoryPoints ?? 0));
        public DateTime RecordedAt { get; set; }
        public ObjectId Winner { get; set; }
        public GameList Player1List { get; set; }
        public GameList Player2List { get; set; }

        public static MatchResult Create(
            SecondaryObjectiveState secondaryObjective,
            PlayerResultDto player1Result,
            PlayerResultDto player2Result,
            GameList player1List, 
            GameList player2List)
        {
            var points = CalculateWinPoints(player1Result.VictoryPoints, player2Result.VictoryPoints);

            var pointsAfteObjective = CalculateSecondaryObjective(
                secondaryObjective,
                points.Player1,
                points.Player2);
            
            return new MatchResult
            {
                MatchId = ObjectId.GenerateNewId(),
                RecordedAt = DateTime.UtcNow,
                SecondaryObjective = secondaryObjective,
                Winner = GetWinnerId(player1Result, player2Result, pointsAfteObjective),
                Player1 = PlayerResult.Create(player1Result.Id, player1Result.VictoryPoints, pointsAfteObjective.Player1),
                Player2 = PlayerResult.Create(player2Result.Id, player2Result.VictoryPoints, pointsAfteObjective.Player2),
                Player1List = player1List,
                Player2List = player2List
            };
        }

        private static ObjectId GetWinnerId(
            PlayerResultDto player1Result,
            PlayerResultDto player2Result,
            PointTuple pointsAfteObjective)
        {
            if (pointsAfteObjective.Player1 == pointsAfteObjective.Player2)
            {
                return player1Result.VictoryPoints > player2Result.VictoryPoints ? player1Result.Id : player2Result.Id;
            }

            return pointsAfteObjective.Player1 > pointsAfteObjective.Player2 ? player1Result.Id : player2Result.Id;
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
                { 3151, new PointTuple(17, 3)},
                { Int32.MaxValue, new PointTuple(17, 3)}
            };

            var pair = pointTuples.First(p => Math.Abs(player1 - player2) <= p.Key);

            return player1 > player2
                ? new PointTuple(pair.Value.Player1, pair.Value.Player2)
                : new PointTuple(pair.Value.Player2, pair.Value.Player1);
        }

        private static PointTuple CalculateSecondaryObjective(
            SecondaryObjectiveState secondaryObjective,
            int points1,
            int points2)
        {
            if (secondaryObjective == SecondaryObjectiveState.player1)
            {
                return new PointTuple(points1 + 3, points2 - 3);
            }

            if (secondaryObjective == SecondaryObjectiveState.player2)
            {
                return new PointTuple(points1 - 3, points2 + 3);
            }

            return new PointTuple(points1, points2);
        }
    }
    
    public class PlayerResult
    {
        public ObjectId Id { get; set; }

        /// <summary>
        /// the points you get by killing enemies
        /// </summary>
        public int VictoryPoints { get; set; }

        /// <summary>
        /// the points you get for secondary objective and victory points difference
        /// </summary>
        public int BattlePoints { get; set; }

        public static PlayerResult Create(ObjectId playerId,
            int battlePoints,
            int winPoints)
        {
            return new()
            {
                VictoryPoints = battlePoints,
                BattlePoints = winPoints,
                Id = playerId
            };
        }
    }
    
    public class MatchResultDto
    {
        public ObjectId MatchId  { get; set; }
        public PlayerResultDto Player1 { get; set; } = new();
        public PlayerResultDto Player2 { get; set; } = new();
        public SecondaryObjectiveState SecondaryObjective { get; set; }
    }

    public class PlayerResultDto
    {
        public ObjectId Id { get; set; }
        [Range(0, 5100)]
        public int VictoryPoints { get; set; }
    }

    public enum SecondaryObjectiveState
    {
        draw = 0,
        player1 = 1,
        player2 = 2
    }
}
