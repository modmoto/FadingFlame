using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FadingFlame.Matchups
{
    public class MatchResultDto
    {
        public ObjectId MatchId  { get; set; }
        public bool WasDefLoss  { get; set; }
        public PlayerResultDto Player1 { get; set; } = new();
        public PlayerResultDto Player2 { get; set; } = new();
        
        /// <summary>
        /// Primary objective, too lazy to change it in DB
        /// </summary>
        public SecondaryObjectiveState SecondaryObjective { get; set; }
        
        /// <summary>
        /// the actual secondary
        /// </summary>
        public SecondaryObjectiveState _3_0_SecondaryObjective { get; set; }
    }

    public enum SecondaryObjectiveState
    {
        draw = 0,
        player1 = 1,
        player2 = 2
    }
}
