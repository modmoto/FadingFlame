using FadingFlame.Repositories;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FadingFlame.Lists
{
    public class Army : IIdentifiable
    {
        public GameList List1 { get; set; }
        public GameList List2 { get; set; }
        [BsonId]
        public ObjectId Id { get; set; }
        public int Season { get; set; }
    }
}