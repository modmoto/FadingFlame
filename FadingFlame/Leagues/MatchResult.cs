using MongoDB.Bson;

namespace FadingFlame.Leagues
{
    public class MatchResult
    {
        public PlayerResult Player2 { get; set; }
        public PlayerResult Player1 { get; set; }
    }

    public class PlayerResult
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