using FadingFlame.Lists;
using FadingFlame.Repositories;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FadingFlame.Players
{
    public class Player : IIdentifiable
    {
        private bool _isRanked;

        [BsonId]
        public ObjectId Id { get; set; }
        public bool IsRanked
        {
            get => MatchCount >= 3;
            set => _isRanked = value;
        }

        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public int MatchCount => Wins + Losses;
        public string DisplayName { get; set; }
        public string AccountEmail { get; set; }
        public string DiscordTag { get; set; }
        [BsonIgnore]
        public Army ArmyCurrentSeason  { get; set; }
        [BsonIgnore]
        public Army ArmyNextSeason  { get; set; }
        public ObjectId ArmyIdCurrentSeason  { get; set; }
        public ObjectId ArmyIdNextSeason  { get; set; }
        public Mmr Mmr  { get; set; }
        public bool SubmittedLists => ArmyCurrentSeason != null;
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


        public void AddWin(Mmr mmr)
        {
            Wins++;
            Mmr = mmr;
        }

        public void AddLoss(Mmr mmr)
        {
            Losses++;
            Mmr = mmr;
        }
        
        public void RemoveWin()
        {
            Wins--;
        }
        
        public void RemoveLoss()
        {
            Losses--;
        }

        public void AddDraw()
        {
            Draws++;
        }

        public void RemoveDraw()
        {
            Draws--;
        }

        public void SubmitLists(GameList list1, GameList list2, int season)
        {
            ArmyCurrentSeason = new Army
            {
                Id = ArmyIdCurrentSeason,
                Season = season,
                List1 = list1,
                List2 = list2
            };
        }

        public void Update(EditUserModel model)
        {
            DiscordTag = model.DiscordTag;
            DisplayName = model.DisplayName;
            Location = Location.Create(model.Country, model.TimeZone);
        }

        public void SetLocalInformation(Location location)
        {
            Location = location;
        }
    }
}