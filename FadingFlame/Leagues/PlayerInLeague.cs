using FadingFlame.Matchups;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FadingFlame.Leagues;

[BsonIgnoreExtraElements]
public class PlayerInLeague
{
    public ObjectId Id { get; set; }
    /// <summary>
    /// the points you get for secondary objective and victory points difference
    /// </summary>
    [BsonIgnore]
    public int BattlePoints { get; set; }
    [BsonIgnore]
    public int GamesCount { get; set; }
    /// <summary>
    /// the points you get by killing enemies
    /// </summary>
    [BsonIgnore]
    public int VictoryPoints { get; set; }
    public int PenaltyPoints { get; set; }

    public void Penalty(int newPenalty)
    {
        PenaltyPoints = newPenalty;
    }
        
    public static PlayerInLeague Create(ObjectId playerId)
    {
        return new()
        {
            Id = playerId
        };
    }

    public void CountPoints(PlayerResult result)
    {
        BattlePoints += result.BattlePoints;
        VictoryPoints += result.VictoryPoints;
        GamesCount++;
    }
}