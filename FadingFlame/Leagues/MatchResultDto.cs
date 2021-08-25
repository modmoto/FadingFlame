using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using MongoDB.Bson;

namespace FadingFlame.Leagues
{
    public class MatchResult
    {
        public ObjectId MatchId  { get; set; }
        public PlayerResult Player1 { get; set; }
        public PlayerResult Player2 { get; set; }
        public DateTimeOffset RecordedAt { get; set; }

        public static MatchResult Create(PlayerResultDto player1Result, PlayerResultDto player2Result)
        {
            var points = CalculateWinPoints(player1Result.BattlePoints, player2Result.BattlePoints);

            var pointsAfteObjective = CalculateSecondaryObjective(
                player1Result,
                player2Result,
                points.Player1,
                points.Player2);
            
            return new MatchResult
            {
                MatchId = ObjectId.GenerateNewId(),
                RecordedAt = DateTimeOffset.UtcNow,
                Player1 = PlayerResult.Create(player1Result.Id, player1Result.BattlePoints, pointsAfteObjective.Player1, player1Result.SecondaryObjective),
                Player2 = PlayerResult.Create(player2Result.Id, player2Result.BattlePoints, pointsAfteObjective.Player2, player2Result.SecondaryObjective),
            };
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
            PlayerResultDto player1ResultDto,
            PlayerResultDto player2ResultDto,
            int points1,
            int points2)
        {
            if (player1ResultDto.SecondaryObjective == SecondaryObjectiveState.won
                && player2ResultDto.SecondaryObjective == SecondaryObjectiveState.lost)
            {
                return new PointTuple(points1 + 3, points2 - 3);
            }

            if (player1ResultDto.SecondaryObjective == SecondaryObjectiveState.lost
                && player2ResultDto.SecondaryObjective == SecondaryObjectiveState.won)
            {
                return new PointTuple(points1 - 3, points2 + 3);
            }

            if (player1ResultDto.SecondaryObjective == SecondaryObjectiveState.draw
                && player2ResultDto.SecondaryObjective == SecondaryObjectiveState.draw)
            {
                return new PointTuple(points1, points2);
            }

            throw new ValidationException("Invalid secondary objective selection");
        }
    }
    
    public class PlayerResult
    {
        public ObjectId Id { get; set; }
        public int BattlePoints { get; set; }
        public int WinPoints { get; set; }
        public SecondaryObjectiveState SecondaryObjective { get; set; }

        public static PlayerResult Create(
            ObjectId playerId,
            int battlePoints, 
            int winPoints, 
            SecondaryObjectiveState secondaryObjective)
        {
            return new()
            {
                BattlePoints = battlePoints,
                WinPoints = winPoints,
                Id = playerId,
                SecondaryObjective = secondaryObjective,
            };
        }
    }
    
    public class MatchResultDto
    {
        public ObjectId MatchId  { get; set; }
        public PlayerResultDto Player1 { get; set; }
        public PlayerResultDto Player2 { get; set; }
    }

    public class PlayerResultDto
    {
        public ObjectId Id { get; set; }
        public int BattlePoints { get; set; }
        public SecondaryObjectiveState SecondaryObjective { get; set; }
    }

    public enum SecondaryObjectiveState
    {
        draw = 0,
        won = 1,
        lost = 2
    }
}