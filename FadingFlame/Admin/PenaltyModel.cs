using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace FadingFlame.Admin
{
    public class PenaltyModel
    {
        public PenaltyModel(ObjectId playerId, int penaltyPoints)
        {
            PlayerId = playerId;
            PenaltyPoints = penaltyPoints;
        }
        
        public ObjectId PlayerId { get; set; }
        [Range(Int32.MinValue, 0)]
        public int PenaltyPoints { get; set; }
    }
}