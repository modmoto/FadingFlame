using System;
using MongoDB.Bson;

namespace FadingFlame.Events
{
    public class UserLoggedInEvent : EventArgs
    {
        public ObjectId PlayerId { get; set; }
    }
}