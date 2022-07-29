using MongoDB.Bson;

namespace FadingFlame.Matchups;

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
    Draw = 0,
    Player1 = 1,
    Player2 = 2
}