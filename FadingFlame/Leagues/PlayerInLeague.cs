using System.Collections.Generic;
using MongoDB.Bson;

namespace FadingFlame.Leagues
{
    public class PlayerInLeague
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public int Points { get; set; }
        public int GmesCount => Games.Count;
        public List<MatchResult> Games { get; set; } = new();

        public void RecordResult(MatchResult match)
        {
            Games.Add(match);
        }

        public void RecordResult(int points)
        {
            Points += points;
        }

        public static PlayerInLeague Create(ObjectId playerId, string name)
        {
            return new()
            {
                Id = playerId,
                Name = name
            };
        }
    }
}