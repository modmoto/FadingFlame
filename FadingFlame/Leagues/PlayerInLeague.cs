using System.Collections.Generic;
using MongoDB.Bson;

namespace FadingFlame.Leagues
{
    public class PlayerInLeague
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public int Points { get; set; }
        public int GamesCount { get; set; }

        public void RecordResult(int points)
        {
            Points += points;
            GamesCount++;
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