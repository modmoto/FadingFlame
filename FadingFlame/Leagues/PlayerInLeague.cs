using MongoDB.Bson;

namespace FadingFlame.Leagues
{
    public class PlayerInLeague
    {
        public ObjectId Id { get; set; }
        /// <summary>
        /// the points you get for secondary objective and victory points difference
        /// </summary>
        public int BattlePoints { get; set; }
        public int GamesCount { get; set; }
        /// <summary>
        /// the points you get by killing enemies
        /// </summary>
        public int VictoryPoints { get; set; }

        public int PenaltyPoints { get; set; }

        public void Penalty(int penalty)
        {
            PenaltyPoints = penalty;
        }
        
        public void RecordResult(int points, int victoryPoints)
        {
            BattlePoints += points;
            VictoryPoints += victoryPoints;
            GamesCount++;
        }
        
        public void DeleteResult(int points, int victoryPoints)
        {
            BattlePoints -= points;
            VictoryPoints -= victoryPoints;
            GamesCount--;
        }

        public static PlayerInLeague Create(ObjectId playerId)
        {
            return new()
            {
                Id = playerId
            };
        }
    }
}