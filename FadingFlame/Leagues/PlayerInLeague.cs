using MongoDB.Bson;

namespace FadingFlame.Leagues
{
    public class PlayerInLeague
    {
        public ObjectId Id { get; set; }
        public int Points { get; set; }
        public int GamesCount { get; set; }
        public int VictoryPoints { get; set; }

        public void RecordResult(int points, int victoryPoints)
        {
            Points += points;
            VictoryPoints += victoryPoints;
            GamesCount++;
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