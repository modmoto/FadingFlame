using System.Collections.Generic;
using FadingFlame.Repositories;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FadingFlame.Players
{
    public class Player : IIdentifiable
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string DisplayName { get; set; }
        public string DiscordTag { get; set; }
        public List<Faction> Armies { get; set; } = new();

        public static Player Create(string name)
        {
            return new()
            {
                DisplayName = name
            };
        }
    }
}