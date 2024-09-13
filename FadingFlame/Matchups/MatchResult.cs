using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FadingFlame.Leagues;
using FadingFlame.Lists;
using FadingFlame.Players;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FadingFlame.Matchups
{
    [BsonIgnoreExtraElements]
    // old MatchId is still in DB
    public class MatchResult
    {
        public PlayerResult Player1 { get; set; }
        public PlayerResult Player2 { get; set; }
        public bool WasDefLoss { get; set; }
        public SecondaryObjectiveState SecondaryObjective { get; set; }
        public SecondaryObjectiveState _3_0_SecondaryObjective { get; set; }
        public int VictoryPointsDifference => Math.Abs((Player1?.VictoryPoints ?? 0) - (Player2?.VictoryPoints ?? 0));
        public DateTime RecordedAt { get; set; }
        public ObjectId Winner { get; set; }
        [BsonIgnore] 
        public ObjectId Looser => IsDraw ? ObjectId.Empty : Winner == Player1.Id ? Player2.Id : Player1.Id; 
        public GameList Player1List { get; set; }
        public GameList Player2List { get; set; }
        public bool IsDraw => Winner == ObjectId.Empty;

        public static async Task<MatchResult> Create(
            IMmrRepository mmrRepository,
            SecondaryObjectiveState primaryObjective,
            SecondaryObjectiveState secondaryObjective,
            Mmr player1Mmr,
            Mmr player2Mmr,
            PlayerResultDto player1Result,
            PlayerResultDto player2Result,
            GameList player1List,
            GameList player2List,
            bool wasDefLoss)
        {
            var points = CalculateWinPoints(player1Result.VictoryPoints, player2Result.VictoryPoints);

            var pointsAfteObjective = CalculateObjective(
                primaryObjective,
                secondaryObjective,
                points.Player1,
                points.Player2);

            var winner = pointsAfteObjective.Player1 == pointsAfteObjective.Player2 
                ? ObjectId.Empty 
                : pointsAfteObjective.Player1 > pointsAfteObjective.Player2 
                    ? player1Result.Id 
                    : player2Result.Id;

            var newMmrs = await CreateNewMmr(mmrRepository, player1Mmr, player2Mmr, player1Result, winner);

            return new MatchResult
            {
                RecordedAt = DateTime.UtcNow,
                SecondaryObjective = primaryObjective,
                _3_0_SecondaryObjective = secondaryObjective,
                Winner = winner,
                WasDefLoss = wasDefLoss,
                Player1 = PlayerResult.Create(player1Result.Id, player1Result.VictoryPoints, pointsAfteObjective.Player1, player1Mmr, newMmrs.Item1),
                Player2 = PlayerResult.Create(player2Result.Id, player2Result.VictoryPoints, pointsAfteObjective.Player2, player2Mmr, newMmrs.Item2),
                Player1List = wasDefLoss ? GameList.DeffLoss() : player1List,
                Player2List = wasDefLoss ? GameList.DeffLoss() : player2List,
            };
        }

        private static async Task<Tuple<Mmr, Mmr>> CreateNewMmr(
            IMmrRepository mmrRepository, 
            Mmr player1Mmr, 
            Mmr player2Mmr,
            PlayerResultDto player1Result, 
            ObjectId winner)
        {
            Mmr player1NewMmr;
            Mmr player2NewMmr;
            if (winner != ObjectId.Empty)
            {
                var player = player1Result.Id == winner ? 1 : 2;
                var mmrsOld = new List<Mmr> {player1Mmr, player2Mmr};
                var mmrsNew = await mmrRepository.UpdateMmrs(UpdateMmrRequest.Create(mmrsOld, player));
                player1NewMmr = mmrsNew[0];
                player2NewMmr = mmrsNew[1];
            }
            else
            {
                player1NewMmr = player1Mmr;
                player2NewMmr = player2Mmr;
            }

            return new Tuple<Mmr, Mmr>(player1NewMmr, player2NewMmr);
        }

        private static PointTuple CalculateWinPoints(int player1, int player2)
        {
            var pointTuples = new Dictionary<int, PointTuple>
            {
                { 200, new PointTuple(10, 10)},
                { 400, new PointTuple(11, 9)},
                { 800, new PointTuple(12, 8)},
                { 1200, new PointTuple(13, 7)},
                { 1600, new PointTuple(14, 6)},
                { 2000, new PointTuple(15, 5)},
                { Int32.MaxValue, new PointTuple(16, 4)}
            };

            var pair = pointTuples.First(p => Math.Abs(player1 - player2) <= p.Key);

            return player1 > player2
                ? new PointTuple(pair.Value.Player1, pair.Value.Player2)
                : new PointTuple(pair.Value.Player2, pair.Value.Player1);
        }

        private static PointTuple CalculateObjective(
            SecondaryObjectiveState primaryObjective,
            SecondaryObjectiveState secondaryObjective,
            int points1,
            int points2)
        {
            var objectivePointsPlayer1 = points1;
            var objectivePointsPlayer2 = points2;

            if (primaryObjective == SecondaryObjectiveState.player1)
            {
                objectivePointsPlayer1 += 3;
                objectivePointsPlayer2 -= 3;
            }
            
            if (primaryObjective == SecondaryObjectiveState.player2)
            {
                objectivePointsPlayer1 -= 3;
                objectivePointsPlayer2 += 3;
            }
            
            if (secondaryObjective == SecondaryObjectiveState.player1)
            {
                objectivePointsPlayer1 += 1;
                objectivePointsPlayer2 -= 1;
            }
            
            if (secondaryObjective == SecondaryObjectiveState.player2)
            {
                objectivePointsPlayer1 -= 1;
                objectivePointsPlayer2 += 1;
            }

            return new PointTuple(objectivePointsPlayer1, objectivePointsPlayer2);
        }

        public static MatchResult ZeroToZero(ObjectId player1Id, ObjectId player2Id)
        {
            return new MatchResult
            {
                WasDefLoss = true,
                RecordedAt = DateTime.UtcNow,
                Player1 = PlayerResult.ZeroToZero(player1Id),
                Player2 = PlayerResult.ZeroToZero(player2Id),
                Winner = ObjectId.Empty,
                SecondaryObjective = SecondaryObjectiveState.draw,
                _3_0_SecondaryObjective = SecondaryObjectiveState.draw,
                Player1List = GameList.DeffLoss(),
                Player2List = GameList.DeffLoss()
            };
        }

        public static async Task<MatchResult> CreateKoResult(
            IMmrRepository mmrRepository,
            SecondaryObjectiveState primaryObjective,
            SecondaryObjectiveState secondaryObjective,
            Mmr player1Mmr,
            Mmr player2Mmr,
            PlayerResultDto player1Result,
            PlayerResultDto player2Result,
            GameList player1List,
            GameList player2List,
            bool wasDefLoss)
        {
            var points = CalculateWinPoints(player1Result.VictoryPoints, player2Result.VictoryPoints);

            var pointsAfteObjective = CalculateObjective(
                primaryObjective,
                secondaryObjective,
                points.Player1,
                points.Player2);

            var winner = pointsAfteObjective.Player1 == pointsAfteObjective.Player2 
                ? player1Result.VictoryPoints > player2Result.VictoryPoints 
                    ? player1Result.Id
                    : player2Result.Id
                : pointsAfteObjective.Player1 > pointsAfteObjective.Player2 
                    ? player1Result.Id 
                    : player2Result.Id;

            var newMmrs = await CreateNewMmr(mmrRepository, player1Mmr, player2Mmr, player1Result, winner);
            
            return new MatchResult
            {
                RecordedAt = DateTime.UtcNow,
                SecondaryObjective = primaryObjective,
                _3_0_SecondaryObjective = secondaryObjective,
                Winner = winner,
                Player1 = PlayerResult.Create(player1Result.Id, player1Result.VictoryPoints, pointsAfteObjective.Player1, player1Mmr, newMmrs.Item1),
                Player2 = PlayerResult.Create(player2Result.Id, player2Result.VictoryPoints, pointsAfteObjective.Player2, player2Mmr, newMmrs.Item2),
                Player1List = player1List,
                Player2List = player2List,
                WasDefLoss = wasDefLoss
            };
        }

        public static MatchResult TempInit()
        {
            var zeroToZero = ZeroToZero(ObjectId.Empty, ObjectId.Empty);
            zeroToZero.Player1.BattlePoints = 10;
            zeroToZero.Player2.BattlePoints = 10;
            return zeroToZero;
        }

        public static MatchResult CreateWithoutMmr(
            SecondaryObjectiveState primaryObjective,
            SecondaryObjectiveState secondaryObjective,
            int player1VictoryPoints,
            int player2VictoryPoints)
        {
            var points = CalculateWinPoints(player1VictoryPoints, player2VictoryPoints);

            var pointsAfteObjective = CalculateObjective(
                primaryObjective,
                secondaryObjective,
                points.Player1,
                points.Player2);
            return new MatchResult
            {
                Player1 = PlayerResult.Create(ObjectId.Empty, player1VictoryPoints, pointsAfteObjective.Player1, null, null),
                Player2 = PlayerResult.Create(ObjectId.Empty, player2VictoryPoints, pointsAfteObjective.Player2, null, null)
            };
        }
    }
}