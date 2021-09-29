using System;
using System.Collections.Generic;
using System.Linq;
using FadingFlame.Admin;
using FadingFlame.Matchups;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FadingFlame.Leagues
{
    public class GameDay
    {
        [BsonIgnore]
        public List<Matchup> Matchups { get; set; }
        public List<ObjectId> MatchupIds { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate => StartDate.AddDays(14);
        public SecondaryObjective SecondaryObjective { get; set; }
        public Deployment Deployment { get; set; }
        public static GameDay Create(DateTime startDate, List<Matchup> matchups)
        {
            return new()
            {
                Matchups = matchups,
                MatchupIds = matchups.Select(m => m.Id).ToList(),
                StartDate = startDate
            };
        }

        public void SetScenarioAndDeployments(SecondaryObjective secondaryObjective, Deployment deployment)
        {
            SecondaryObjective = secondaryObjective;
            Deployment = deployment;
        }
    }
}