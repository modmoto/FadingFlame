using System.Collections.Generic;

namespace FadingFlame.Leagues
{
    public class GameDay
    {
        public List<Matchup> Matchups { get; set; }

        public static GameDay Create(List<Matchup> matchups)
        {
            return new()
            {
                Matchups = matchups
            };
        }
    }
}