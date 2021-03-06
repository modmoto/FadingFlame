using System;
using System.Collections.Generic;
using FadingFlame.Leagues;
using FadingFlame.Matchups;
using MongoDB.Bson;

namespace FadingFlame.Playoffs
{
    public class Round
    {
        public DateTime DeadLine { get; set; }
        public List<Matchup> Matchups { get; set; }
        public List<ObjectId> MatchupIds { get; set; } = new();

        public static Round Create(List<PlayerInLeague> playerIds)
        {
            var matchups = new List<Matchup>();
            var matchupIds = new List<ObjectId>();

            for (var index = 0; index < playerIds.Count; index += 2)
            {
                var matchup = Matchup.CreateForPlayoff(ObjectId.GenerateNewId(), playerIds[index], playerIds[index + 1]);
                matchups.Add(matchup);
                matchupIds.Add(matchup.Id);
            }

            return new Round
            {
                Matchups = matchups,
                MatchupIds = matchupIds,
            };
        }
    }
}