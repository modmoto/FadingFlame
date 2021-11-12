using MongoDB.Bson;

namespace FadingFlame.Matchups
{
    public class MatchResultDto
    {
        public ObjectId MatchId  { get; set; }
        public bool WasDefLoss  { get; set; }
        public PlayerResultDto Player1 { get; set; } = new();
        public PlayerResultDto Player2 { get; set; } = new();
        public SecondaryObjectiveState SecondaryObjective { get; set; }
    }

    public enum SecondaryObjectiveState
    {
        draw = 0,
        player1 = 1,
        player2 = 2
    }
}
