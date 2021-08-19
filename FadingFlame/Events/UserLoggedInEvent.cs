using System;
using MongoDB.Bson;

namespace FadingFlame.Events
{
    public class UserLoggedInEvent : EventArgs
    {
        public UserLoggedInEvent(ObjectId playerId)
        {
            PlayerId = playerId;
        }

        public ObjectId PlayerId { get; set; }
    }
}