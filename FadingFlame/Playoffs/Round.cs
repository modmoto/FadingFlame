using System.Collections.Generic;
using FadingFlame.Leagues;

namespace FadingFlame.Playoffs
{
    public class Round
    {
        public List<Matchup> Matchups { get; set; }

        public static Round Create(List<PlayerInLeague> playerIds)
        {
            var matchups = new List<Matchup>();

            for (var index = 0; index < playerIds.Count; index += 2)
            {
                var matchup = Matchup.Create(playerIds[index], playerIds[index + 1]);
                matchups.Add(matchup);
            }

            return new Round
            {
                Matchups = matchups
            };
        }
    }
}