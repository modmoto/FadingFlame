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
        public bool IsFinished => Result != null;

        public static Matchup Create(PlayerInLeague playerAtHome, PlayerInLeague playerAsGuest)
        {
            return new()
            {
                Player1 = playerAtHome.Id,
                Player2 = playerAsGuest.Id,
                MatchId = ObjectId.GenerateNewId()
            };
        }
        
        public void SelectList(ObjectId playerId, string listName)
        {
            if (Player1 == playerId)
            {
                Player1List = listName;
            }
            
            if (Player2 == playerId)
            {
                Player2List = listName;
            }
        }

        public void RecordResult(MatchResult result)
        {
            if (IsFinished)
            {
                throw new ValidationException("Match allready reported");
            }

            Result = result;
        }

        public void DeleteResult()
        {
            Result = null;
        }
    }
}