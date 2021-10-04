using FadingFlame.Lists;
using FadingFlame.Repositories;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FadingFlame.Players
{
    [BsonIgnoreExtraElements]
    public class PlayerLegacy : IIdentifiable
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string DisplayName { get; private set; }
        public string AccountEmail { get; private set; }
        public string DiscordTag { get; set; }
        public Army Army  { get; set; }
        public Mmr Mmr  { get; set; }
        public bool SubmittedLists => Army != null;
        public Location Location { get; set; }
    }
}