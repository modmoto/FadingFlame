using FadingFlame.Lists;
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
        public string AccountEmail { get; set; }
        public string DiscordTag { get; set; }
        [BsonIgnore]
        public Army ArmyCurrentSeason  { get; set; }
        [BsonIgnore]
        public Army ArmyNextSeason  { get; set; }
        public ObjectId ArmyIdCurrentSeason  { get; set; }
        public ObjectId ArmyIdNextSeason  { get; set; }
        public Mmr Mmr  { get; set; }
        public bool SubmittedLists => ArmyIdCurrentSeason != default;
        public Location Location { get; set; }
        public bool SubmitedListsNextSeason => ArmyIdNextSeason != default;
        public int? SelfAssessment { get; set; }

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

        public void SubmitListsNextSeason(GameList list1, GameList list2, int season, int? selfAssessment)
        {
            if (selfAssessment != null)
            {
                SelfAssessment = selfAssessment;
            }
            ArmyNextSeason = new Army
            {
                Id = ArmyIdNextSeason,
                Season = season,
                List1 = list1,
                List2 = list2
            };
        }

        public void SubmitLateList(GameList list1, GameList list2, int season)
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

        public void TrackBackMmr(Mmr mmrDifference)
        {
            Mmr = new Mmr
            {
                Rating = Mmr.Rating - mmrDifference.Rating,
                RatingDeviation = Mmr.RatingDeviation - mmrDifference.RatingDeviation,
            };
        }

        public void RetractParticipationNextSeason()
        {
            ArmyNextSeason = null;
            ArmyIdNextSeason = default;
        }

        public void Enroll()
        {
            ArmyCurrentSeason = ArmyNextSeason;
            ArmyIdCurrentSeason = ArmyIdNextSeason;
            ArmyNextSeason = default;
            ArmyIdNextSeason = default;
        }
    }
}