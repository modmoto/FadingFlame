using FadingFlame.Players;
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
        public Mmr OldMmr { get; set; }
        public Mmr NewMmr { get; set; }
        
        public static PlayerResult Create(
            ObjectId playerId,
            int battlePoints,
            int winPoints,
            Mmr oldMmr,
            Mmr newMmr)
        {
            return new()
            {
                VictoryPoints = battlePoints,
                BattlePoints = winPoints,
                Id = playerId,
                OldMmr = oldMmr,
                NewMmr = newMmr
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