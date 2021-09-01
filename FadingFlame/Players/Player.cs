using System.ComponentModel.DataAnnotations;
using FadingFlame.Repositories;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FadingFlame.Players
{
    public class Player : IIdentifiable
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string AccountName { get; private set; }
        public string DiscordTag { get; set; }
        public SeasonArmy Army  { get; set; }
       
        public static Player Create(string name)
        {
            return new()
            {
                AccountName = name
            };
        }

        public void SubmitLists(Faction faction, GameList list1, GameList list2)
        {
            if (Army != null) throw new ValidationException("Lists already set for this season");
            Army = new SeasonArmy
            {
                List1 = list1,
                List2 = list2,
                Faction = faction
            };
        }

        public void ResetLists()
        {
            Army = null;
        }
    }

    public class SeasonArmy
    {
        public GameList List1 { get; set; }
        public GameList List2 { get; set; }
        public Faction Faction { get; set; }
    }
}