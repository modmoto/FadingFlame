using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace FadingFlame.Leagues
{
    public class Matchup
    {
        public ObjectId MatchId { get; set; }
        public ObjectId Player1 { get; set; }
        public ObjectId Player2 { get; set; }
        public string Name1 { get; set; }
        public string Name2 { get; set; }
        public MatchResult Result { get; set; }

        public static Matchup Create(PlayerInLeague playerAtHome, PlayerInLeague playerAsGuest)
        {
            return new()
            {
                Player1 = playerAtHome.Id,
                Player2 = playerAsGuest.Id,
                Name1 = playerAtHome.Name,
                Name2 = playerAsGuest.Name,
                MatchId = ObjectId.GenerateNewId()
            };
        }

        public void RecordResult(MatchResult result)
        {
            if (Result != null)
            {
                throw new ValidationException("Match allready reported");
            }

            Result = result;
        }
    }
}