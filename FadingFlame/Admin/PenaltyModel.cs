using MongoDB.Bson;

namespace FadingFlame.Admin;

public class PenaltyModel
{
    public PenaltyModel(ObjectId playerId, int penaltyPoints)
    {
        PlayerId = playerId;
        PenaltyPoints = penaltyPoints;
    }
        
    public ObjectId PlayerId { get; set; }
    public int PenaltyPoints { get; set; }
}