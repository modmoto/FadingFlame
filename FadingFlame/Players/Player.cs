using System.Globalization;
using FadingFlame.Repositories;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FadingFlame.Players
{
    public class Player : IIdentifiable
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string DisplayName { get; private set; }
        public string AccountEmail { get; private set; }
        public string DiscordTag { get; set; }
        public SeasonArmy Army  { get; set; }
        public Mmr Mmr  { get; set; }
        public bool SubmittedLists => Army != null;
        public Location Location { get; set; }

        public static Player Create(string name, string accountMail)
        {
            return new()
            {
                DisplayName = name,
                AccountEmail = accountMail,
                Mmr = Mmr.Create()
            };
        }


        public void UpdateMmr(Mmr mmr)
        {
            Mmr = mmr;
        }

        public void SubmitLists(GameList list1, GameList list2)
        {
            Army = new SeasonArmy
            {
                List1 = list1,
                List2 = list2
            };
        }

        public void Update(EditUserModel model)
        {
            DiscordTag = model.DiscordTag;
            DisplayName = model.DisplayName;
            if (model.Country == "nono" && Location != null)
            {
                Location.Country = null;
            }
            else
            {
                if (Location != null) Location.Country = new RegionInfo(model.Country);
            }
            
            if (model.TimeZone == "nono" && Location != null)
            {
                Location.TimezoneRaw = null;
            }
            else
            {
                if (Location != null) Location.TimezoneRaw = model.TimeZone;
            }
        }

        public void SetLocalInformation(Location location)
        {
            Location = location;
        }
    }

    public class SeasonArmy
    {
        public GameList List1 { get; set; }
        public GameList List2 { get; set; }
    }
}