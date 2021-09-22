using System;
using System.Collections.Generic;

namespace FadingFlame.Leagues
{
    public class GameDay
    {
        public List<Matchup> Matchups { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate => StartDate.AddDays(14);
        public static GameDay Create(DateTime startDate, List<Matchup> matchups)
        {
            return new()
            {
                Matchups = matchups,
                StartDate = startDate
            };
        }
    }
}