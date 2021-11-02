using MongoDB.Bson;

namespace FadingFlame.Matchups
{
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

        public static PlayerResult ZeroToZero(ObjectId playerId)
        {
            return new ()
            {
                Id = playerId
            };
        }
    }
}