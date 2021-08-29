using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace FadingFlame.Leagues
{
    public class Matchup
    {
        public ObjectId MatchId { get; set; }
        public ObjectId Player1 { get; set; }
        public ObjectId Player2 { get; set; }
        public MatchResult Result { get; set; }
        public bool PlayersSelectedList => !string.IsNullOrEmpty(Player1List) && !string.IsNullOrEmpty(Player2List);
        public string Player1List { get; set; }
        public string Player2List { get; set; }

        public static Matchup Create(PlayerInLeague playerAtHome, PlayerInLeague playerAsGuest)
        {
            return new()
            {
                Player1 = playerAtHome.Id,
                Player2 = playerAsGuest.Id,
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