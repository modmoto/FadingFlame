using MongoDB.Bson;

namespace FadingFlame.Leagues
{
    public class Matchup
    {
        public ObjectId PlayerAtHome { get; set; }
        public ObjectId PlayerAsGuest { get; set; }
        public string NameAtHome { get; set; }
        public string NameAsGuest { get; set; }

        public static Matchup Create(PlayerInLeague playerAtHome, PlayerInLeague playerAsGuest)
        {
            return new()
            {
                PlayerAtHome = playerAtHome.Id,
                PlayerAsGuest = playerAsGuest.Id,
                NameAtHome = playerAtHome.Name,
                NameAsGuest = playerAsGuest.Name
            };
        }
    }
}