using System.Text.Json.Serialization;
using FadingFlame.Players;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FadingFlame.Matchups;

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

    [JsonIgnore]
    [BsonIgnore]
    public Mmr MmrDifference => new()
    {
        Rating = NewMmr.Rating - OldMmr.Rating,
        RatingDeviation = NewMmr.RatingDeviation - OldMmr.RatingDeviation
    };

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