using System.Collections.Generic;
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
        public string DisplayName { get; set; }
        public string DiscordTag { get; set; }
        public GameList List1 { get; set; }
        public GameList List2 { get; set; }
        public List<Faction> Armies { get; set; } = new();
        public string AccountEmail { get; set; }

        public static Player Create(string name, string email)
        {
            return new()
            {
                DisplayName = name,
                AccountEmail = email
            };
        }

        public void SubmitLists(GameList list1, GameList list2)
        {
            if (List1 != null || List2 != null) throw new ValidationException("Lists already set for this season");
            List1 = list1;
            List2 = list2;
        }

        public void ResetLists()
        {
            List1 = null;
            List2 = null;
        }
    }
}