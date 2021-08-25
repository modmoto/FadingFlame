using System;
using System.Collections.Generic;

namespace FadingFlame.Leagues
{
    public class GameDay
    {
        public List<Matchup> Matchups { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate => StartDate.AddDays(14);
        public static GameDay Create(DateTimeOffset startDate, List<Matchup> matchups)
        {
            return new()
            {
                Matchups = matchups,
                StartDate = startDate
            };
        }
    }
}